using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DocumentInvoice.Domain;
using DocumentInvoice.Infrastructure;
using DocumentInvoice.Infrastructure.Repository;
using DocumentInvoice.Service.Command;
using MediatR;
using Microsoft.Extensions.Options;

namespace DocumentInvoice.Service.Handlers.Command;

public class PersistDocumentInfoCommandHandler : IRequestHandler<PersistDocumentInfoCommand, Unit>
{
    private readonly ApplicationSettings _configuration;
    private readonly IRepositoryFactory<DocumentInvoiceContext> _repositoryFactory;
    private readonly IRepository<Document> _documentRepo;
    private readonly IRepository<Company> _companyRepo;
    private readonly BlobServiceClient _blobServiceClient;

    public PersistDocumentInfoCommandHandler(IOptions<ApplicationSettings> configuration, IRepositoryFactory<DocumentInvoiceContext> repositoryFactory, BlobServiceClient blobServiceClient)
    {
        _configuration = configuration.Value;
        _repositoryFactory = repositoryFactory;
        _documentRepo = _repositoryFactory.GetRepository<Document>();
        _companyRepo = _repositoryFactory.GetRepository<Company>();
        _blobServiceClient = blobServiceClient;
    }

    public async Task<Unit> Handle(PersistDocumentInfoCommand request, CancellationToken cancellationToken)
    {
        BlobContainerClient sourceContainerClient = _blobServiceClient.GetBlobContainerClient(_configuration.ContainerName);

        BlobClient sourceBlobClient = sourceContainerClient.GetBlobClient(request.Message);

        BlobProperties blobProperties = await sourceBlobClient.GetPropertiesAsync();

        var documentOwnerId = int.Parse(blobProperties.Metadata["OwnerId"]);
        var company = _companyRepo.Query.SingleOrDefault(x => x.Id == documentOwnerId);

        BlobContainerClient destinationContainerClient = _blobServiceClient.GetBlobContainerClient(company.ContainerName);

        BlobClient destinationBlobClient = destinationContainerClient.GetBlobClient(request.Message);
        await destinationContainerClient.CreateIfNotExistsAsync();
        await destinationBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri);

        await WaitForCopyToCompleteAsync(destinationBlobClient);

        if (company != null)
        {
            await _documentRepo.AddAsync(new Document
            {
                DocumentName = blobProperties.Metadata["DocumentName"],
                DocumentFileName = blobProperties.Metadata["DocumentFileName"],
                DocumentCategory = Int32.Parse(blobProperties.Metadata["DocumentCategory"].ToString()),
                UploadTime = DateTime.Parse(blobProperties.Metadata["UploadTime"]),
                Customer = company,
                CompanyId = company.Id,
                Container = company.ContainerName,
                Month = blobProperties.Metadata["Month"],
                Year = blobProperties.Metadata["Year"],
                Url = Uri.UnescapeDataString(destinationBlobClient.Uri.ToString()),
            }, cancellationToken);

            await _repositoryFactory.SaveChangesAsync(cancellationToken);
        }

        await sourceBlobClient.DeleteIfExistsAsync();


        return Unit.Value;
    }

    static async Task WaitForCopyToCompleteAsync(BlobClient blobClient)
    {
        while (true)
        {
            var copyStatus = await blobClient.GetPropertiesAsync();
            if (copyStatus.Value.CopyStatus != CopyStatus.Pending)
            {
                break;
            }

            await Task.Delay(1000);
        }
    }
}
