using DocumentInvoice.Service.Command;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.Net;

namespace DocumentInvoice.Api
{
    public class UserFunction
    {
        private readonly IMediator _mediator;

        public UserFunction(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function(nameof(CreateUser))]
        [OpenApiOperation(
            operationId: "create.user",
            tags: new[] { "User" },
            Summary = "Create a new user",
            Description = "Creates a new user with bearer authentication token flow via header",
            Visibility = OpenApiVisibilityType.Important
        )]
        [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(CreateUserCommand), Required = true, Description = "User creation data")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Unit), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(string), Description = "The Unauthorized response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Description = "Internal server error")]
        public async Task<HttpResponseData> CreateUser([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user")] HttpRequestData req,
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
            var request = JsonConvert.DeserializeObject<CreateUserCommand>(requestBody);
            var result = await _mediator.Send(request);
            await response.WriteAsJsonAsync(result);

            return response;
        }
    }
}
