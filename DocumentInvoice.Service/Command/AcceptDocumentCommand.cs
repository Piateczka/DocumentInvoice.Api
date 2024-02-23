using DocumentInvoice.Service.Enums;
using MediatR;

namespace DocumentInvoice.Service.Command;

public class AcceptDocumentCommand : IRequest<bool>
{
    public int DocumentId { get; set; }
    public DocumentStatus DocumentStatus { get; set; }
    public string Comment { get; set; }
}
