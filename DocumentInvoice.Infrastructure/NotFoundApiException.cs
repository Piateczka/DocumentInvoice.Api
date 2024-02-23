using System.Net;

namespace DocumentInvoice.Infrastructure;

public class NotFoundApiException : ApiException
{
    private int _httpStatusCode = (int)HttpStatusCode.BadRequest;
    public NotFoundApiException() { }
    public NotFoundApiException(string message) : base(message) { }
}
