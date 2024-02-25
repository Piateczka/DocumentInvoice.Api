using DocumentInvoice.Domain;
using Microsoft.Azure.Functions.Worker;
using System.Net;
using System.Reflection;

namespace DocumentInvoice.Api.Extensions
{
    public static class FunctionContextExtensions
    {

        public static void SetHttpResponseStatusCode(this FunctionContext context, HttpStatusCode statusCode)
        {
            // Terrible reflection code since I haven't found a nicer way to do this...
            // For some reason the types are marked as internal
            // If there's code that will break in this sample,
            // it's probably here.
            var coreAssembly = Assembly.Load("Microsoft.Azure.Functions.Worker.Core");
            var featureInterfaceName = "Microsoft.Azure.Functions.Worker.Context.Features.IFunctionBindingsFeature";
            var featureInterfaceType = coreAssembly.GetType(featureInterfaceName);
            var bindingsFeature = context.Features.Single(
                f => f.Key.FullName == featureInterfaceType.FullName).Value;
            var invocationResultProp = featureInterfaceType.GetProperty("InvocationResult");

            var grpcAssembly = Assembly.Load("Microsoft.Azure.Functions.Worker.Grpc");
            var responseDataType = grpcAssembly.GetType("Microsoft.Azure.Functions.Worker.GrpcHttpResponseData");
            var responseData = Activator.CreateInstance(responseDataType, context, statusCode);

            invocationResultProp.SetMethod.Invoke(bindingsFeature, new object[] { responseData });
        }

        public static MethodInfo GetTargetFunctionMethod(this FunctionContext context)
        {
            // This contains the fully qualified name of the method
            // E.g. IsolatedFunctionAuth.TestFunctions.ScopesAndAppRoles
            var entryPoint = context.FunctionDefinition.EntryPoint;

            var assemblyPath = context.FunctionDefinition.PathToAssembly;
            var assembly = Assembly.LoadFrom(assemblyPath);
            var typeName = entryPoint.Substring(0, entryPoint.LastIndexOf('.'));
            var type = assembly.GetType(typeName);
            var methodName = entryPoint.Substring(entryPoint.LastIndexOf('.') + 1);
            var method = type.GetMethod(methodName);
            return method;
        }

        public static bool IsAdmin(this FunctionContext context)
        {
            if (!context.Items.ContainsKey("role"))
            {
                return false;
            }
            if (context.Items["role"] is not string role)
            {
                return false;
            }

            return role == "Admin";
        }

        public static bool IsAccountant(this FunctionContext context)
        {
            if (!context.Items.ContainsKey("role"))
            {
                return false;
            }
            if (context.Items["role"] is not string role)
            {
                return false;
            }

            return role == "Accountant";
        }

        public static Users GetUserInfo(this FunctionContext context)
        {
            if (context.Items["user"] is not Users user)
            {
                return new Users();
            }

            return user;
        }
    }
}

