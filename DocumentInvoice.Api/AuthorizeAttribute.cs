namespace DocumentInvoice.Api
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute
    {
        /// <summary>
        /// Defines which user roles are accpeted.
        /// Must be combined with <see cref="Scopes"/>.
        /// </summary>
        public string[] UserRoles { get; set; } = Array.Empty<string>();
    }
}
