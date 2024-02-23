using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DocumentInvoice.Domain;
using DocumentInvoice.Infrastructure;
using DocumentInvoice.Infrastructure.Repository;
using DocumentInvoice.Service.Command;
using DocumentInvoice.Service.DTO;
using DocumentInvoice.Service.Enums;
using MediatR;
using Microsoft.Extensions.Options;

namespace DocumentInvoice.Service.Handlers.Command;

public class UpdateDocumentCommandHandler : IRequestHandler<UpdateDocumentCommand, DocumentResponse>
{

    private readonly IRepositoryFactory<DocumentInvoiceContext> _repository;
    private readonly IRepository<Document> _documentRepository;
    private readonly BlobServiceClient _blobServiceClient;

    public UpdateDocumentCommandHandler(IRepositoryFactory<DocumentInvoiceContext> repository, BlobServiceClient blobServiceClient)
    {
        _repository = repository;
        _documentRepository = _repository.GetRepository<Document>();
        _blobServiceClient = blobServiceClient;
    }
    public async Task<DocumentResponse> Handle(UpdateDocumentCommand request, CancellationToken cancellationToken)
    {
        Document document = null;
        if (request.IsAdmin)
        {
            document = _documentRepository.Query.FirstOrDefault(x => x.Id == request.Id);
        }
        else
        {
            document = _documentRepository.Query.FirstOrDefault(x => x.Id == request.Id && request.CompanyId.Contains(x.Customer.Id));
        }

        if (document == null)
        {
            throw new KeyNotFoundException("Document not found");
        }

        if (document.DocumentStatus == (int)DocumentStatus.Valid)
        {
            throw new InvalidOperationException("The document status has been set to accepted, documents in this status cannot be modyfi.");
        }

        document.DocumentName = request.DocumentName;
        document.DocumentCategory = (int)request.DocumentCategory;
        document.Month = request.Month;
        document.Year = request.Year;

        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(document.Container);
        BlobClient blobClient = containerClient.GetBlobClient(document.DocumentFileName);
        var blobProperties = await blobClient.GetPropertiesAsync();

        // Access metadata
        var metadata = blobProperties.Value.Metadata;
        using Stream fileStream = request.File.OpenReadStream();
        await blobClient.UploadAsync(fileStream, new BlobUploadOptions
        {
            Metadata = metadata,
            HttpHeaders = new BlobHttpHeaders { ContentType = request.File.ContentType }
        });
        _documentRepository.Update(document);
        await _repository.SaveChangesAsync(cancellationToken);

        var response = new DocumentResponse
        {
            Id = document.Id,
            Commnet = document.Comment,
            DocumentCategory = (DocumentCategory)document.DocumentCategory,
            DocumentName = document.DocumentName,
            DocumentStatus = (DocumentStatus)document.DocumentStatus
        };

        return response;
    }
}
