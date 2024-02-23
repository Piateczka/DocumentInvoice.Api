using System.Net;

namespace DocumentInvoice.Infrastructure;

public class ApiException : System.Exception
{
    private int _httpStatusCode = (int)HttpStatusCode.InternalServerError;
    public ApiException() { }
    public ApiException(string message) : base(message) { }

    public int httpStatusCode
    {
        get { return this._httpStatusCode; }
    }
}
