using DocumentInvoice.Service.DTO;
using MediatR;

namespace DocumentInvoice.Service.Query;

public class GetDocumentsQuery : IRequest<List<DocumentResponse>>
{
    public List<int>? CompanyId { get; set; }
    public bool IsAdmin { get; set; }
}
