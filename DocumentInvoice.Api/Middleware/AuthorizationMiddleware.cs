using DocumentInvoice.Api.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using System.Net;
using System.Reflection;

namespace DocumentInvoice.Api.Middleware
{
    public class AuthorizationMiddleware : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var internalCall = bool.Parse(context.Items["internalCall"].ToString());
            if (internalCall)
                await next(context);

            var role = context.Items["role"].ToString();
            if (role == null)
            {
                context.SetHttpResponseStatusCode(HttpStatusCode.Forbidden);
                return;
            }

            if (!CheckRole(context, role))
            {
                context.SetHttpResponseStatusCode(HttpStatusCode.Forbidden);
                return;
            }

            await next(context);
        }

        private static bool CheckRole(FunctionContext context, string role)
        {
            var targetMethod = context.GetTargetFunctionMethod();

            var acceptedAppRoles = GetAcceptedAppRoles(targetMethod);
            var hasAcceptedRole = acceptedAppRoles.Any(ur => ur.Contains(role));
            return hasAcceptedRole;
        }

        private static List<string> GetAcceptedAppRoles(MethodInfo targetMethod)
        {
            var attributes = GetCustomAttributesOnClassAndMethod<AuthorizeAttribute>(targetMethod);
            // Same as above for scopes and user roles,
            // only allow app roles that are common in
            // class and method level attributes.

            return attributes
                .Select(a => a.UserRoles)
                .Aggregate(new List<string>().AsEnumerable(), (result, acceptedRoles) =>
                {
                    return acceptedRoles;
                })
                .ToList();
        }

        private static List<T> GetCustomAttributesOnClassAndMethod<T>(MethodInfo targetMethod)
    where T : Attribute
        {
            var methodAttributes = targetMethod.GetCustomAttributes<T>();
            var classAttributes = targetMethod.DeclaringType.GetCustomAttributes<T>();
            return methodAttributes.Concat(classAttributes).ToList();
        }
    }
}
