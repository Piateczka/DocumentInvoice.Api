using Azure.Security.KeyVault.Secrets;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using DocumentInvoice.Infrastructure;
using DocumentInvoice.Service.Command;
using DocumentInvoice.Service.DTO;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace DocumentInvoice.Service.Handlers.Command;

public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, DocumentResponse>
{
    private readonly ApplicationSettings _configuration;
    private readonly BlobServiceClient _blobServiceClient;

    public CreateDocumentCommandHandler(IOptions<ApplicationSettings> configuration, BlobServiceClient blobServiceClient)
    {
        _configuration = configuration.Value;
        _blobServiceClient = blobServiceClient;
    }

    public async Task<DocumentResponse> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_configuration.ContainerName);

        await containerClient.CreateIfNotExistsAsync();

        using Stream fileStream = request.File.OpenReadStream();
        var fileExtension = Path.GetExtension(request.File.FileName);
        var documentFileName = $"{request.DocumentName}#{Guid.NewGuid().ToString().Substring(0, 6)}{fileExtension}";
        BlobClient blobClient = containerClient.GetBlobClient(documentFileName);

        Dictionary<string, string> metadata = new Dictionary<string, string>
        {
            { "FileExtension", fileExtension},
            { "DocumentFileName", documentFileName },
            { "DocumentName", request.DocumentName },
            { "OwnerId", request.OwnerId.ToString() },
            { "DocumentCategory", ((int)request.DocumentCategory).ToString() },
            { "UploadTime", request.UploadTime.ToString() },
            { "Month", request.Month.ToString() },
            { "Year", request.Year.ToString() },
        };

        await blobClient.UploadAsync(fileStream, new BlobUploadOptions
        {
            Metadata = metadata,
            HttpHeaders = new BlobHttpHeaders { ContentType = request.File.ContentType }
        });

        BlobSasBuilder sasBuilder = new BlobSasBuilder()
        {
            BlobContainerName = _configuration.ContainerName,
            BlobName = documentFileName,
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
            Path = string.Format("{0}/{1}", _configuration.ContainerName, documentFileName),
            Query = sasToken
        };

        var storageAccount = CloudStorageAccount.Parse(_configuration.BlobConnectionString);
        var queueClient = storageAccount.CreateCloudQueueClient();
        var queue = queueClient.GetQueueReference("documenttoprocess");
        await queue.CreateIfNotExistsAsync();
        var message = new CloudQueueMessage(documentFileName);
        await queue.AddMessageAsync(message);

        return new DocumentResponse
        {
            DocumentName = documentFileName,
            DocumentCategory = request.DocumentCategory,
            DocumentStatus = request.Status,
            Url = fulluri.Uri.ToString()
        };
    }
}
