using DocumentInvoice.Service.DTO;
using MediatR;

namespace DocumentInvoice.Service.Query;

public class GetDocumentQuery : Base<DocumentResponse>
{
    public int DocumentId { get; set; }
}
