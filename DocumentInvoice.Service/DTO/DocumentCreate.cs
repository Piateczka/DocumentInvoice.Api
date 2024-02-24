namespace DocumentInvoice.Service.DTO
{
    public class DocumentCreate
    {
        public int DocumentId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentFileName { get; set; }
        public string Comment { get; set; }
        public string Container { get; set; }
        public string Month{ get; set; }
        public string Year { get; set; }
    }
}
