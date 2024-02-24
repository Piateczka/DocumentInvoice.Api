using DocumentInvoice.Service.DTO;
using MediatR;

namespace DocumentInvoice.Service.Query;

public class GetDocumentQuery : BaseRequest<DocumentResponse>
{
    public int DocumentId { get; set; }
}
