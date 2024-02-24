using MediatR;

namespace DocumentInvoice.Service.Command
{
    public class DeleteTagDocumentCommand : Base<Unit>
    {
        public int DocumentId { get; set; }
        public int TagId { get; set; }
    }
}
