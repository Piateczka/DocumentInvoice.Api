using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace DocumentInvoice.Api
{
    public class FunctionTest
    {

        [Function("Function")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "test")] HttpRequest req)
        {
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
