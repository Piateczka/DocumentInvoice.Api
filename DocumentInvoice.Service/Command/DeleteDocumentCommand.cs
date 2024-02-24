using MediatR;

namespace DocumentInvoice.Service.Command;

public class DeleteDocumentCommand : BaseRequest<Unit>
{
    public int DocumentId { get; set; }
}
