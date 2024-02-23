using DocumentInvoice.Domain;
using DocumentInvoice.Service.DTO;
using DocumentInvoice.Service.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DocumentInvoice.Service.Command;

public class CreateDocumentCommand : IRequest<DocumentResponse>
{
    public IFormFile File { get; set; }
    public string DocumentName { get; set; }
    public int OwnerId { get; set; }
    public DateTime UploadTime { get; set; }
    public DocumentCategory DocumentCategory { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public DocumentStatus Status = DocumentStatus.New;
}
