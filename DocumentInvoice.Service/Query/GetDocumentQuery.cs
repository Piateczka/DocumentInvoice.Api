using DocumentInvoice.Service.DTO;
using MediatR;

namespace DocumentInvoice.Service.Query;

public class GetDocumentQuery : IRequest<DocumentResponse>
{
    public List<int>? CompanyId { get; set; }
    public int DocumentId { get; set; }
    public bool IsAdmin { get; set; }
}
