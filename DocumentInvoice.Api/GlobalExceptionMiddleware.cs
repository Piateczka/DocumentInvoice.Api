using DocumentInvoice.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DocumentInvoice.Api
{
    public class GlobalExceptionMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger _logger;

        public GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger)
        {

            _logger = logger;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleException(context, ex);
            }
        }

        private async Task HandleException(FunctionContext context, Exception ex)
        {
            var req = await context.GetHttpRequestDataAsync();
            var res = req.CreateResponse();

            if (ex is AggregateException aggregateException)
            {
                foreach (var innerException in aggregateException.InnerExceptions)
                {
                    if (innerException is NotFoundApiException)
                    {
                        await res.WriteAsJsonAsync(innerException.Message, HttpStatusCode.NotFound);
                        context.GetInvocationResult().Value = res;
                        return; // Return to avoid processing other inner exceptions
                    }
                    if (innerException is InvalidOperationException)
                    {
                        await res.WriteAsJsonAsync(innerException.Message, HttpStatusCode.MethodNotAllowed);
                        context.GetInvocationResult().Value = res;
                        return; // Return to avoid processing other inner exceptions
                    }
                }
            }
            else
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await res.WriteAsJsonAsync("An error occurred", HttpStatusCode.InternalServerError);
            }

            context.GetInvocationResult().Value = res;
        }
    }
}
