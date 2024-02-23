using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DocumentInvoice.Api
{
    public class HealthCheckServiceFunction
    {
        private readonly HealthCheckService _healthCheck;

        public HealthCheckServiceFunction(HealthCheckService healthCheck)
        {
            _healthCheck = healthCheck;
        }

        [Function(nameof(HealthCheckServiceFunction))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "healthcheck")] HttpRequestData req)   
        {
            var healthStatus = await _healthCheck.CheckHealthAsync();
            return new OkObjectResult(Enum.GetName(typeof(HealthStatus), healthStatus.Status));
        }
    }
}
