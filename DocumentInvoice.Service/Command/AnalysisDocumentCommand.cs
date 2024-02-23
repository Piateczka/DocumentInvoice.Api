using MediatR;

namespace DocumentInvoice.Service.Command;

public class AnalysisDocumentCommand : IRequest<Unit>
{
    public string DocumentName { get; set; }
    public int DocumentId { get; set; }
}
