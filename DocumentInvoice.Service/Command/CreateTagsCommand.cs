using DocumentInvoice.Service.DTO;
using MediatR;

namespace DocumentInvoice.Service.Command;

public class CreateTagsCommand : IRequest<DocumentResponse>
{
    public int DocumentId { get; set; }
    public string[] Tags { get; set; }
}
