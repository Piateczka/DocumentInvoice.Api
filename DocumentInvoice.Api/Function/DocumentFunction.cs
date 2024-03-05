using DocumentInvoice.Api.Extensions;
using DocumentInvoice.Service;
using DocumentInvoice.Service.Command;
using DocumentInvoice.Service.DTO;
using DocumentInvoice.Service.Enums;
using DocumentInvoice.Service.Query;
using HttpMultipartParser;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.Net;

namespace DocumentInvoice.Api.Function
{
    public class DocumentFunction
    {
        private readonly ILogger<AutomaticFunction> _logger;
        private readonly IMediator _mediator;

        public DocumentFunction(IMediator mediator, ILogger<AutomaticFunction> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [Function(nameof(UploadDocument))]
        [OpenApiOperation(
            operationId: "upload.document",
            tags: new[] { "Document" },
            Summary = "Upload document",
            Visibility = OpenApiVisibilityType.Important
        )]
        [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiRequestBody(contentType: "multipart/form-data", bodyType: typeof(CreateDocumentRequest), Required = true, Description = "Document data")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(DocumentResponse), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(string), Description = "The Unauthorized response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Description = "Internal server error")]
        [Authorize(UserRoles = new[] { "User", "Admin" })]
        public async Task<HttpResponseData> UploadDocument([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "document")] HttpRequestData req)
        {
            var parsedFormBody = await MultipartFormDataParser.ParseAsync(req.Body);
            var file = parsedFormBody.Files.First();
            IFormFile formFile = new StreamFormFile(file.Data, file.FileName, file.ContentType);
            var request = new CreateDocumentCommand
            {
                DocumentCategory = Helpers.Helpers.ParseEnum<DocumentCategory>(parsedFormBody.GetParameterValue("documentCategory")),
                DocumentName = parsedFormBody.GetParameterValue("documentName"),
                UploadTime = DateTime.Now,
                OwnerId = int.Parse(parsedFormBody.GetParameterValue("ownerId")),
                Month = int.Parse(parsedFormBody.GetParameterValue("month")),
                Year = int.Parse(parsedFormBody.GetParameterValue("year")),
                File = formFile
            };
            var result = await _mediator.Send(request);
            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(result);

            return response;
        }

        [Function(nameof(AddTagsToDocument))]
        [OpenApiOperation(
            operationId: "add.tags.document",
            tags: new[] { "Document" },
            Summary = "Add tags to document",
            Visibility = OpenApiVisibilityType.Important
        )]
        [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(string[]), Required = true, Description = "Tags data")]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "ID of the document")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string[]), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(string), Description = "The Unauthorized response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Description = "Internal server error")]
        [Authorize(UserRoles = new[] { "Accountant", "Admin" })]
        public async Task<HttpResponseData> AddTagsToDocument([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "document/{id:int}")] HttpRequestData req, int id)
        {
            var requestBody = await req.ReadAsStringAsync();
            var tags = JsonConvert.DeserializeObject<string[]>(requestBody);
            var request = new CreateTagsCommand
            {
                DocumentId = id,
                Tags = tags
            };
            var result = await _mediator.Send(request);
            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(result);

            return response;
        }

        [Function(nameof(RemoveTagsFromDocument))]
        [OpenApiOperation(
            operationId: "remove.tags.document",
            tags: new[] { "Document" },
            Summary = "Remove tags to document",
            Visibility = OpenApiVisibilityType.Important
        )]
        [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiParameter(name: "tagId", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "ID of the tag")]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "ID of the document")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string[]), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(string), Description = "The Unauthorized response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Description = "Internal server error")]
        [Authorize(UserRoles = new[] { "Accountant", "Admin" })]
        public async Task<HttpResponseData> RemoveTagsFromDocument([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "document/{id:int}/tag/{tagId:int}")] HttpRequestData req, int id,
            int tagId, FunctionContext context)
        {
            var request = new DeleteTagDocumentCommand
            {
                DocumentId = id,
                TagId = tagId
            };

            var result = await _mediator.Send(request);
            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(result);

            return response;
        }

        [Function(nameof(GetListDocuments))]
        [OpenApiOperation(
            operationId: "get.documents",
            tags: new[] { "Document" },
            Summary = "Retrieve documents",
            Visibility = OpenApiVisibilityType.Important
        )]
        [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<DocumentResponse>), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(string), Description = "The Unauthorized response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Description = "Internal server error")]
        [Authorize(UserRoles = new[] { "User", "Admin", "Accountant" })]
        public async Task<HttpResponseData> GetListDocuments([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documentList")] HttpRequestData req,
            FunctionContext context)
        {
            var request = new GetDocumentsQuery();
            var rbacInfo = context.Items["RBACInfo"] as RBACInfo;
            request.RBACInfo = rbacInfo;
            var result = await _mediator.Send(request);
            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(result);

            return response;
        }

        [Function(nameof(GetDocument))]
        [OpenApiOperation(
            operationId: "get.document",
            tags: new[] { "Document" },
            Summary = "Retrieve document by ID",
            Visibility = OpenApiVisibilityType.Important
        )]
        [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "ID of the document")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(DocumentResponse), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(string), Description = "The Unauthorized response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Description = "Internal server error")]
        [Authorize(UserRoles = new[] { "User", "Admin", "Accountant" })]
        public async Task<HttpResponseData> GetDocument([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "document/{id:int}")] HttpRequestData req, int id,
            FunctionContext context)
        {
            var request = new GetDocumentQuery();
            request.DocumentId = id;
            var rbacInfo = context.Items["RBACInfo"] as RBACInfo;
            request.RBACInfo = rbacInfo;

            var result = await _mediator.Send(request);

            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(result);

            return response;
        }

        [Function(nameof(DeleteDocument))]
        [OpenApiOperation(
            operationId: "delete.document",
            tags: new[] { "Document" },
            Summary = "Delete document by ID",
            Visibility = OpenApiVisibilityType.Important
        )]
        [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "ID of the document")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NoContent, contentType: "application/json", bodyType: typeof(Unit), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(string), Description = "The Unauthorized response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Description = "Internal server error")]
        [Authorize(UserRoles = new[] { "User", "Admin" })]
        public async Task<HttpResponseData> DeleteDocument([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "document/{id:int}")] HttpRequestData req, int id,
            FunctionContext context)
        {
            var request = new DeleteDocumentCommand();
            request.DocumentId = id;
            var rbacInfo = context.Items["RBACInfo"] as RBACInfo;
            request.RBACInfo = rbacInfo;

            var result = await _mediator.Send(request);

            var response = req.CreateResponse();
            await req.CreateResponse().WriteAsJsonAsync("Document deleted", HttpStatusCode.NoContent);

            return response;
        }

        [Function(nameof(UpdateDocument))]
        [OpenApiOperation(
            operationId: "update.document",
            tags: new[] { "Document" },
            Summary = "Update document information",
            Visibility = OpenApiVisibilityType.Important
        )]
        [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiRequestBody(contentType: "multipart/form-data", bodyType: typeof(UpdateDocumentRequest), Required = true, Description = "Document data")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(DocumentResponse), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(string), Description = "The Unauthorized response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Description = "Internal server error")]
        [Authorize(UserRoles = new[] { "User", "Admin" })]
        public async Task<HttpResponseData> UpdateDocument([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "document")] HttpRequestData req,
            FunctionContext context)
        {
            var parsedFormBody = await MultipartFormDataParser.ParseAsync(req.Body);
            var file = parsedFormBody.Files.First();
            IFormFile formFile = new StreamFormFile(file.Data, file.FileName, file.ContentType);
            var request = new UpdateDocumentCommand
            {
                DocumentCategory = (DocumentCategory)Enum.Parse(typeof(DocumentCategory), parsedFormBody.GetParameterValue("documentCategory")),
                DocumentName = parsedFormBody.GetParameterValue("documentName"),
                Month = parsedFormBody.GetParameterValue("month"),
                Year = parsedFormBody.GetParameterValue("year"),
                File = formFile,
                Id = int.Parse(parsedFormBody.GetParameterValue("documentId")),

            };
            var rbacInfo = context.Items["RBACInfo"] as RBACInfo;
            request.RBACInfo = rbacInfo;
            var result = await _mediator.Send(request);
            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(result);

            return response;
        }

        [Function(nameof(VerificationDocument))]
        [OpenApiOperation(
          operationId: "verification.document",
          tags: new[] { "Document" },
          Summary = "Verify document",
          Visibility = OpenApiVisibilityType.Important
        )]
        [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(AcceptDocumentCommand), Required = true, Description = "Document data")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(bool), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(string), Description = "The Unauthorized response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Description = "Internal server error")]
        [Authorize(UserRoles = new[] { "Accountant", "Admin" })]
        public async Task<HttpResponseData> VerificationDocument([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "document/verification")] HttpRequestData req, string id)
        {
            var requestBody = await req.ReadAsStringAsync();
            var request = JsonConvert.DeserializeObject<AcceptDocumentCommand>(requestBody);
            var result = await _mediator.Send(request);

            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(result);

            return response;
        }

        [Function(nameof(SearchDocument))]
        [OpenApiOperation(
            operationId: "search.document",
            tags: new[] { "Document" },
            Summary = "Fulltext search document",
            Description = "Fulltext search document",
            Visibility = OpenApiVisibilityType.Important
        )]
        [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiParameter(name: "Query", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Value to search")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<DocumentResponse>), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(string), Description = "The Unauthorized response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Description = "Internal server error")]
        [Authorize(UserRoles = new[] { "User", "Admin", "Accountant" })]
        public async Task<HttpResponseData> SearchDocument([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "document/search")] HttpRequestData req,
            FunctionContext context)
        {
            var request = new SearchDocumentQuery()
            {
                Query = req.Query["Query"]
            };
            var rbacInfo = context.Items["RBACInfo"] as RBACInfo;
            request.RBACInfo = rbacInfo;
            var result = await _mediator.Send(request);

            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(result);

            return response;
        }

        [Function(nameof(FilterDocument))]
        [OpenApiOperation(
            operationId: "filter.document",
            tags: new[] { "Document" },
            Summary = "Filter document",
            Description = "Filterh document",
            Visibility = OpenApiVisibilityType.Important
        )]
        [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiParameter(name: "Tag", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Tag to search")]
        [OpenApiParameter(name: "Month", In = ParameterLocation.Query, Required = false, Type = typeof(int?), Description = "Month to search")]
        [OpenApiParameter(name: "Year", In = ParameterLocation.Query, Required = false, Type = typeof(int?), Description = "Year to search")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<DocumentResponse>), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(string), Description = "The Unauthorized response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Description = "Internal server error")]
        [Authorize(UserRoles = new[] { "User", "Admin", "Accountant" })]
        public async Task<HttpResponseData> FilterDocument([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "document/filter")] HttpRequestData req,
            FunctionContext context)
        {
            int month;
            int year;
            var request = new FilterDocumentQuery()
            {
                Tag = req.Query["Tag"],
                Month = int.TryParse(req.Query["Month"], out month) ? month : 0,
                Year = int.TryParse(req.Query["Year"], out year) ? year : 0
            };
            var rbacInfo = context.Items["RBACInfo"] as RBACInfo;
            request.RBACInfo = rbacInfo;
            var result = await _mediator.Send(request);

            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(result);

            return response;
        }

    }
}
