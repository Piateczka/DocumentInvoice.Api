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
    private readonly IRepository<DocumentTag> _documentTagRepository;
    private readonly ApplicationSettings _configuration;
    private readonly BlobServiceClient _blobServiceClient;

    public GetDocumentQueryHandler(IRepositoryFactory<DocumentInvoiceContext> repository, IOptions<ApplicationSettings> configuration, BlobServiceClient blobServiceClient)
    {
        _repository = repository;
        _documentRepository = _repository.GetRepository<Document>();
        _documentTagRepository = _repository.GetRepository<DocumentTag>();
        _configuration = configuration.Value;
        _blobServiceClient = blobServiceClient;
    }
    public async Task<DocumentResponse> Handle(GetDocumentQuery request, CancellationToken cancellationToken)
    {

        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_configuration.ContainerName);
        DocumentResponse document = null;
        if (request.RBACInfo.IsAdminOrAccountant)
        {
            document = await _documentRepository.Query.GroupJoin(
                _documentTagRepository.Query,
                x => x.Id,
                y => y.Document.Id,
                (x, y) => new { Document = x, Tags = y })
                .Where(a => a.Document.Id == request.DocumentId)
                    .Select(x => new DocumentResponse
                    {
                        Id = x.Document.Id,
                        Commnet = x.Document.Comment,
                        DocumentCategory = (Enums.DocumentCategory)x.Document.DocumentCategory,
                        DocumentName = x.Document.DocumentName,
                        DocumentStatus = (Enums.DocumentStatus)x.Document.DocumentStatus,
                        ContainerName = x.Document.Container,
                        Tags = x.Tags.Where(t=>t.IsActive).Select(x => new TagResponse
                        {
                            Tag = x.Tag,
                            TagId = x.Id
                        }).ToArray()
                    }).FirstOrDefaultAsync(cancellationToken);
        }
        else
        {
            document = await _documentRepository.Query.GroupJoin(
                _documentTagRepository.Query,
                x => x.Id,
                y => y.Document.Id,
                (x, y) => new { Document = x, Tags = y })
                .Where(a => request.RBACInfo.UserCompanyIdList.Contains(a.Document.Id) && a.Document.Id == request.DocumentId).Select(x => new DocumentResponse
                {
                    Id = x.Document.Id,
                    Commnet = x.Document.Comment,
                    DocumentCategory = (Enums.DocumentCategory)x.Document.DocumentCategory,
                    DocumentName = x.Document.DocumentName,
                    DocumentStatus = (Enums.DocumentStatus)x.Document.DocumentStatus,
                    ContainerName = x.Document.Container,
                    Tags = x.Tags.Where(t => t.IsActive).Select(x => new TagResponse
                    {
                        Tag = x.Tag,
                        TagId = x.Id
                    }).ToArray()
                }).FirstOrDefaultAsync(cancellationToken);
        }

        if (document == null)
        {
            throw new NotFoundApiException("Document not found");
        }

        BlobSasBuilder sasBuilder = new BlobSasBuilder()
        {
            BlobContainerName = document.ContainerName,
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
            Path = string.Format("{0}/{1}", document.ContainerName.ToLower(), document.DocumentName),
            Query = sasToken
        };

        return document;

    }
}
