using DocumentInvoice.Service.DTO;
using DocumentInvoice.Service.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DocumentInvoice.Service.Command;

public class UpdateDocumentCommand : Base<DocumentResponse>
{
    public int Id { get; set; }
    public IFormFile File { get; set; }
    public string DocumentName { get; set; }
    public DocumentCategory DocumentCategory { get; set; }
    public string Month { get; set; }
    public string Year { get; set; }
}
