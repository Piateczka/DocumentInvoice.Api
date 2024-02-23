using DocumentInvoice.Service.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DocumentInvoice.Service.DTO;

public class CreateDocumentRequest
{
    public byte[] FileUpload { get; set; }
    public string DocumentName { get; set; }
    public int OwnerId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public DocumentCategory DocumentCategory { get; set; }
}
