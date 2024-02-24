using MediatR;

namespace DocumentInvoice.Service.Command;

public class DeleteDocumentCommand : Base<Unit>
{
    public int DocumentId { get; set; }
}
