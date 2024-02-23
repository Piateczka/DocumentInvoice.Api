using DocumentInvoice.Domain;
using DocumentInvoice.Infrastructure;
using DocumentInvoice.Infrastructure.Repository;
using Google.Apis.Auth;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace DocumentInvoice.Api
{
    public class AuthenticationMiddleware : IFunctionsWorkerMiddleware
    {

        private readonly IRepositoryFactory<DocumentInvoiceContext> _repository;
        private readonly IRepository<Users> _userRepository;
        public AuthenticationMiddleware(IRepositoryFactory<DocumentInvoiceContext> repository)
        {
            _repository = repository;
            _userRepository = _repository.GetRepository<Users>();
        }


        public async Task Invoke(
            FunctionContext context, FunctionExecutionDelegate next)
        {
            //TODO FunctionContext -> Claims Identity
            if (IsHttpRequest(context))
            {
                if (!TryGetTokenFromHeaders(context, out var token))
                {
                    await next(context);
                }
                try
                {
                    var payload = await GoogleJsonWebSignature.ValidateAsync(token);

                    var user = _userRepository.Query
                                .Include(x=>x.Role)
                                .Include(x => x.UserAccessList)
                                .ThenInclude(y => y.Company)
                                .FirstOrDefault(c => c.Email == payload.Email);

                    if (user == null)
                    {
                        return;
                    }

                    context.Items.Add("role", user.Role.Name);
                    context.Items.Add("email", payload.Email);
                    context.Items.Add("user", user);
                    context.Items.Add("isAuthenticated", true);

                    await next(context);
                }
                catch (Exception ex)
                {
                    context.Items.Add("error", ex.Message);
                    await next(context);
                }
            }
            else
            {
                // Do nothing for non-HTTP triggers (e.g., QueueTrigger)
                await next(context);
            }


        }

        private static bool IsHttpRequest(FunctionContext context)
        {
            // Assuming HTTP request triggers have "HttpRequest" binding data
            return context.BindingContext.BindingData.ContainsKey("HttpRequest") || context.BindingContext.BindingData.ContainsKey("Headers");
        }

        private static bool TryGetTokenFromHeaders(FunctionContext context, out string token)
        {
            token = null;

            if (context.BindingContext.BindingData.TryGetValue("QueueTrigger", out var queueObj))
            {
                return true;
            }
            if (!context.BindingContext.BindingData.TryGetValue("Headers", out var headersObj))
            {
                return false;
            }

            if (headersObj is not string headersStr)
            {
                return false;
            }

            var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headersStr);
            var normalizedKeyHeaders = headers.ToDictionary(h => h.Key.ToLowerInvariant(), h => h.Value);
            if (!normalizedKeyHeaders.TryGetValue("authorization", out var authHeaderValue))
            {
                return false;
            }

            if (!authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            token = authHeaderValue.Substring("Bearer ".Length).Trim();
            return true;
        }

    }
}
