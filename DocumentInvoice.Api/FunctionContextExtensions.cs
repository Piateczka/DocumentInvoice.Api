using DocumentInvoice.Domain;
using Microsoft.Azure.Functions.Worker;

namespace DocumentInvoice.Api
{
    public static class FunctionContextExtensions
    {
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

        public static bool IsUser(this FunctionContext context)
        {
            if (!context.Items.ContainsKey("role"))
            {
                return false;
            }
            if (context.Items["role"] is not string role)
            {
                return false;
            }

            return role == "User";
        }

        public static bool IsAuthenticated(this FunctionContext context)
        {
            if (!context.Items.ContainsKey("isAuthenticated"))
            {
                return false;
            }
            if (context.Items["isAuthenticated"] is not bool authenticated)
            {
                return false;
            }

            return authenticated;
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

