using DocumentInvoice.Service.Enums;

namespace DocumentInvoice.Service.DTO;

public class DocumentResponse
{
    public int Id { get; set; }
    public string DocumentName { get; set; }
    public DocumentCategory DocumentCategory { get; set; }
    public DocumentStatus DocumentStatus { get; set; }
    public string Commnet { get; set; }
    public string Url { get; set; }
}
