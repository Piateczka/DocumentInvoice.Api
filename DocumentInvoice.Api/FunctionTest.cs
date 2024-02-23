using DocumentInvoice.Service.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;

namespace DocumentInvoice.Api
{
    public class FunctionTest
    {

        [Function("Function")]
        [OpenApiOperation(
            operationId: "test",
            tags: new[] { "Test" },
            Visibility = OpenApiVisibilityType.Important
        )]

        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(DocumentResponse), Description = "The OK response")]

        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "test")] HttpRequest req)
        {
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
