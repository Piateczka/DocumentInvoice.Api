using DocumentInvoice.Service.DTO;
using MediatR;

namespace DocumentInvoice.Service.Query;

public class SearchDocumentQuery : BaseRequest<List<DocumentResponse>>
{
    public string Query { get; set; }
}
