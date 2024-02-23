using MediatR;

namespace DocumentInvoice.Service.Command;

public class PersistDocumentInfoCommand : IRequest<Unit>
{
    public string Message { get; set; }
}
