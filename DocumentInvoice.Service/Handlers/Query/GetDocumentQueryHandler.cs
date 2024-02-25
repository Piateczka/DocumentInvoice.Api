using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using DocumentInvoice.Domain;
using DocumentInvoice.Infrastructure;
using DocumentInvoice.Infrastructure.Repository;
using DocumentInvoice.Service.DTO;
using DocumentInvoice.Service.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DocumentInvoice.Service.Handlers.Query;

public class GetDocumentQueryHandler : IRequestHandler<GetDocumentQuery, DocumentResponse>
{
    private readonly IRepositoryFactory<DocumentInvoiceContext> _repository;
    private readonly IRepository<Document> _documentRepository;
    private readonly ApplicationSettings _configuration;
    private readonly BlobServiceClient _blobServiceClient;

    public GetDocumentQueryHandler(IRepositoryFactory<DocumentInvoiceContext> repository, IOptions<ApplicationSettings> configuration, BlobServiceClient blobServiceClient)
    {
        _repository = repository;
        _documentRepository = _repository.GetRepository<Document>();
        _configuration = configuration.Value;
        _blobServiceClient = blobServiceClient;
    }
    public async Task<DocumentResponse> Handle(GetDocumentQuery request, CancellationToken cancellationToken)
    {

        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_configuration.ContainerName);
        Document document = null;
        if (request.RBACInfo.IsAdminOrAccountant)
        {
            document = await _documentRepository.Query.Include(x => x.Customer)
                                .FirstOrDefaultAsync(x => x.Id == request.DocumentId);
        }
        else
        {
            document = await _documentRepository.Query.Include(x => x.Customer)
                    .FirstOrDefaultAsync(x => request.RBACInfo.UserCompanyIdList.Contains(x.CompanyId) && x.Id == request.DocumentId);
        }

        if (document == null)
        {
            throw new NotFoundApiException("Document not found");
        }

        BlobSasBuilder sasBuilder = new BlobSasBuilder()
        {
            BlobContainerName = document.Customer.ContainerName,
            BlobName = document.DocumentName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
            ExpiresOn = DateTime.MaxValue
        };
        var storageCrdentials = new StorageSharedKeyCredential(_blobServiceClient.AccountName, _configuration.StorageAccountAccessKey);
        sasBuilder.SetPermissions(BlobSasPermissions.Read);
        string sasToken = sasBuilder.ToSasQueryParameters(storageCrdentials).ToString();

        UriBuilder fulluri = new UriBuilder()
        {
            Scheme = "https",
            Host = string.Format("upskillsa.blob.core.windows.net"),
            Path = string.Format("{0}/{1}", document.Customer.Name.Replace(" ", "").ToLower(), document.DocumentName),
            Query = sasToken
        };

        return new DocumentResponse
        {
            Id = document.Id,
            DocumentCategory = (Enums.DocumentCategory)document.DocumentCategory,
            DocumentStatus = (Enums.DocumentStatus)document.DocumentStatus,
            Commnet = document.Comment,
            DocumentName = document.DocumentName,
            Url = fulluri.Uri.ToString(),
        };

    }
}
