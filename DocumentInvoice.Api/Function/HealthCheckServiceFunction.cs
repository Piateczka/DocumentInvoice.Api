﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DocumentInvoice.Api.Function
{
    public class HealthCheckServiceFunction
    {
        private readonly HealthCheckService _healthCheck;

        public HealthCheckServiceFunction(HealthCheckService healthCheck)
        {
            _healthCheck = healthCheck;
        }

        [Function(nameof(HealthCheckServiceFunction))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "check")] HttpRequestData req)
        {
            var healthStatus = await _healthCheck.CheckHealthAsync();
            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(Enum.GetName(typeof(HealthStatus), healthStatus.Status));

            return response;
        }
    }
}
