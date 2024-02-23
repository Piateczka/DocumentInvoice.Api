using DocumentInvoice.Service.DTO;
using MediatR;

namespace DocumentInvoice.Service.Query;

public class SearchDocumentQuery : IRequest<List<DocumentResponse>>
{
    public string Query { get; set; }
    public List<int>? CompanyId { get; set; }
    public bool IsAdmin { get; set; }
}
