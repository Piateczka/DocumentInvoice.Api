using Azure.Storage.Blobs;
using DocumentInvoice.Domain;
using DocumentInvoice.Infrastructure;
using DocumentInvoice.Infrastructure.Repository;
using DocumentInvoice.Service.Command;
using DocumentInvoice.Service.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DocumentInvoice.Service.Handlers.Command;

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, Unit>
{
    private readonly IRepositoryFactory<DocumentInvoiceContext> _repository;
    private readonly IRepository<Document> _documentRepository;
    private readonly BlobServiceClient _blobServiceClient;

    public DeleteDocumentCommandHandler(IRepositoryFactory<DocumentInvoiceContext> repository, BlobServiceClient blobServiceClient)
    {
        _repository = repository;
        _documentRepository = _repository.GetRepository<Document>();
        _blobServiceClient = blobServiceClient;
    }
    public async Task<Unit> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        Document document = null;
        if (request.RBACInfo.IsAdminOrAccountant)
        {
            document = await _documentRepository.Query
                .FirstOrDefaultAsync(x => x.Id == request.DocumentId);
        }
        else
        {
            document = await _documentRepository.Query
                .FirstOrDefaultAsync(x => request.RBACInfo.UserCompanyIdList.Contains(x.CompanyId) && x.Id == request.DocumentId);
        }
;

        if (document == null)
        {
            throw new NotFoundApiException("Document not found");
        }

        if (document.DocumentStatus == (int)DocumentStatus.Valid)
        {
            throw new InvalidOperationException("The document status has been set to accepted, documents in this status cannot be deleted.");
        }

        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(document.Container);

        BlobClient blobClient = containerClient.GetBlobClient(document.DocumentName);

        _documentRepository.Delete(document);
        await _repository.SaveChangesAsync(cancellationToken);
        await blobClient.DeleteIfExistsAsync();

        return Unit.Value;


    }
}
