using MediatR;

namespace DocumentInvoice.Service.Command;

public class DeleteDocumentCommand : IRequest<Unit>
{
    public List<int>? CompanyId { get; set; }
    public int DocumentId { get; set; }
    public bool IsAdmin { get; set; }
}
