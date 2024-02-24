using DocumentInvoice.Service.DTO;

namespace DocumentInvoice.Service.Query
{
    public class FilterDocumentQuery :  BaseRequest<List<DocumentResponse>>
    {
        public string Tag { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
