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

namespace DocumentInvoice.Api
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
            Description = "Uploads a document with bearer authentication token flow via header",
            Visibility = OpenApiVisibilityType.Important
        )]
        [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiRequestBody(contentType: "multipart/form-data", bodyType: typeof(CreateDocumentRequest), Required = true, Description = "Document data")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(DocumentResponse), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(string), Description = "The Unauthorized response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Description = "Internal server error")]
        public async Task<HttpResponseData> UploadDocument([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "document")] HttpRequestData req,
            FunctionContext context)
        {
            var response = req.CreateResponse();
            if (!context.IsAuthenticated())
            {
                await response.WriteAsJsonAsync("Unauthorized access", HttpStatusCode.Unauthorized);
                return response;
            }

            //if (!context.IsInRole("User") || !context.IsInRole("Admin"))
            //{
            //    return req.CreateResponse(HttpStatusCode.Forbidden);
            //}

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
            await response.WriteAsJsonAsync(result);

            return response;
        }

        [Function(nameof(GetListDocuments))]
        [OpenApiOperation(
            operationId: "get.document",
            tags: new[] { "Document" },
            Summary = "Retrieve documents",
            Description = "Retrieves a documents with bearer authentication token flow via header",
            Visibility = OpenApiVisibilityType.Important
        )]
        [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<DocumentResponse>), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(string), Description = "The Unauthorized response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Description = "Internal server error")]
        public async Task<HttpResponseData> GetListDocuments([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documentList")] HttpRequestData req,
            FunctionContext context)
        {
            var response = req.CreateResponse();
            if (!context.IsAuthenticated())
            {
                await response.WriteAsJsonAsync("Unauthorized access", HttpStatusCode.Unauthorized);
                return response;
            }
            var request = new GetDocumentsQuery();
            request.IsAdmin = context.IsAdmin();
            if (!request.IsAdmin)
            {
                request.CompanyId = context.GetUserInfo().UserAccessList.Select(x => x.CompanyId).ToList();
            }


            var result = await _mediator.Send(request);
            await response.WriteAsJsonAsync(result);

            return response;
        }

        [Function(nameof(GetDocument))]
        [OpenApiOperation(
            operationId: "get.document",
            tags: new[] { "Document" },
            Summary = "Retrieve document by ID",
            Description = "Retrieves a document by its ID with bearer authentication token flow via header",
            Visibility = OpenApiVisibilityType.Important
        )]
        [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "ID of the document")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(DocumentResponse), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(string), Description = "The Unauthorized response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Description = "Internal server error")]
        public async Task<HttpResponseData> GetDocument([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "document/{id:int}")] HttpRequestData req, int id,
            FunctionContext context)
        {
            var response = req.CreateResponse();
            if (!context.IsAuthenticated())
            {
                await response.WriteAsJsonAsync("Unauthorized access", HttpStatusCode.Unauthorized);
                return response;
            }
            var request = new GetDocumentQuery();
            request.DocumentId = id;
            request.IsAdmin = context.IsAdmin();
            if (!request.IsAdmin)
            {
                request.CompanyId = context.GetUserInfo().UserAccessList.Select(x => x.CompanyId).ToList();
            }


            var result = await _mediator.Send(request);

            await response.WriteAsJsonAsync(result);

            return response;
        }

        [Function(nameof(DeleteDocument))]
        [OpenApiOperation(
            operationId: "delete.document",
            tags: new[] { "Document" },
            Summary = "Delete document by ID",
            Description = "Deletes a document by its ID with bearer authentication token flow via header",
            Visibility = OpenApiVisibilityType.Important
        )]
        [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "ID of the document")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NoContent, contentType: "application/json", bodyType: typeof(Unit), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(string), Description = "The Unauthorized response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Description = "Internal server error")]
        public async Task<HttpResponseData> DeleteDocument([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "document/{id:int}")] HttpRequestData req, int id,
            FunctionContext context)
        {
            var response = req.CreateResponse();
            if (!context.IsAuthenticated())
            {
                await response.WriteAsJsonAsync("Unauthorized access", HttpStatusCode.Unauthorized);
                return response;
            }
            var request = new DeleteDocumentCommand();
            request.DocumentId = id;
            request.IsAdmin = context.IsAdmin();
            if (!request.IsAdmin)
            {
                request.CompanyId = context.GetUserInfo().UserAccessList.Select(x => x.CompanyId).ToList();
            }

            var result = await _mediator.Send(request);

            await response.WriteAsJsonAsync("Document deleted", HttpStatusCode.NoContent);

            return response;
        }

        [Function(nameof(UpdateDocument))]
        [OpenApiOperation(
            operationId: "update.document",
            tags: new[] { "Document" },
            Summary = "Update document information",
            Description = "Updates document information with bearer authentication token flow via header",
            Visibility = OpenApiVisibilityType.Important
        )]
        [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiRequestBody(contentType: "multipart/form-data", bodyType: typeof(UpdateDocumentRequest), Required = true, Description = "Document data")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(DocumentResponse), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(string), Description = "The Unauthorized response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Description = "Internal server error")]
        public async Task<HttpResponseData> UpdateDocument([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "document")] HttpRequestData req,
            FunctionContext context)
        {
            var response = req.CreateResponse();
            if (!context.IsAuthenticated())
            {
                await response.WriteAsJsonAsync("Unauthorized access", HttpStatusCode.Unauthorized);
                return response;
            }

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
            request.IsAdmin = context.IsAdmin();
            if (!request.IsAdmin)
            {
                request.CompanyId = context.GetUserInfo().UserAccessList.Select(x => x.CompanyId).ToList();
            }

            var result = await _mediator.Send(request);
            await response.WriteAsJsonAsync(result);

            return response;
        }

        [OpenApiOperation(
          operationId: "verification.document",
          tags: new[] { "Document" },
          Summary = "Verify document",
          Description = "Verifies document information with bearer authentication token flow via header",
          Visibility = OpenApiVisibilityType.Important
        )]
        [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(AcceptDocumentCommand), Required = true, Description = "Document data")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(bool), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(string), Description = "The Unauthorized response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Description = "Internal server error")]

        [Function(nameof(VerificationDocument))]
        public async Task<HttpResponseData> VerificationDocument([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "document/verification")] HttpRequestData req, string id,
            FunctionContext context)
        {
            var response = req.CreateResponse();
            if (!context.IsAuthenticated())
            {
                await response.WriteAsJsonAsync("Unauthorized access", HttpStatusCode.Unauthorized);
                return response;
            }

            if (!context.IsAdmin())
            {
                return req.CreateResponse(HttpStatusCode.Forbidden);
            }

            var requestBody = await req.ReadAsStringAsync();
            var request = JsonConvert.DeserializeObject<AcceptDocumentCommand>(requestBody);
            var result = await _mediator.Send(request);

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
        [OpenApiParameter(name: "q", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Value to search")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<DocumentResponse>), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(string), Description = "The Unauthorized response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Description = "Internal server error")]
        public async Task<HttpResponseData> SearchDocument([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "document/search")] HttpRequestData req,
            FunctionContext context)
        {
            var response = req.CreateResponse();
            if (!context.IsAuthenticated())
            {
                await response.WriteAsJsonAsync("Unauthorized access", HttpStatusCode.Unauthorized);
                return response;
            }
            var request = new SearchDocumentQuery()
            {
                Query = req.Query["q"]
            };
            request.IsAdmin = context.IsAdmin();
            if (!request.IsAdmin)
            {
                request.CompanyId = context.GetUserInfo().UserAccessList.Select(x => x.CompanyId).ToList();
            }
            var result = await _mediator.Send(request);

            await response.WriteAsJsonAsync(result);

            return response;
        }

    }
}
