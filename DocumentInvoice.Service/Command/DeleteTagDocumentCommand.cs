using MediatR;

namespace DocumentInvoice.Service.Command
{
    public class DeleteTagDocumentCommand : BaseRequest<Unit>
    {
        public int DocumentId { get; set; }
        public int TagId { get; set; }
    }
}
