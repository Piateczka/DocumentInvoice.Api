using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using DocumentInvoice.Domain;
using DocumentInvoice.Infrastructure;
using DocumentInvoice.Infrastructure.Repository;
using DocumentInvoice.Service.Command;
using DocumentInvoice.Service.DTO;
using DocumentInvoice.Service.Enums;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace DocumentInvoice.Service.Handlers.Command;

public class AcceptDocumentCommandHandler : IRequestHandler<AcceptDocumentCommand, bool>
{
    private readonly ApplicationSettings _configuration;
    private readonly IRepositoryFactory<DocumentInvoiceContext> _repositoryFactory;
    private readonly IRepository<Document> _documentRepo;

    public AcceptDocumentCommandHandler(IRepositoryFactory<DocumentInvoiceContext> repositoryFactory, IOptions<ApplicationSettings> configuration)
    {
        _repositoryFactory = repositoryFactory;
        _documentRepo = _repositoryFactory.GetRepository<Document>();
        _configuration = configuration.Value;
    }

    public async Task<bool> Handle(AcceptDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = _documentRepo.Query.Where(x => x.Id == request.DocumentId).FirstOrDefault();

        if (document == null)
        {
            throw new KeyNotFoundException("Document not found");
        }

        document.DocumentStatus = (int)request.DocumentStatus;
        document.Comment = request.Comment;
        if(document.DocumentCategory == (int)DocumentCategory.Other)
        {
            await _repositoryFactory.SaveChangesAsync(cancellationToken);

            return true;
        }
        if (request.DocumentStatus == DocumentStatus.Valid)
        {
            var queueMessage = new DocumentAnalyzeProcess
            {
                DocumentId = document.Id,
                DocumentName = document.DocumentFileName
            };

            var storageAccount = CloudStorageAccount.Parse(_configuration.BlobConnectionString);
            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference("documenttoanalysis");
            await queue.CreateIfNotExistsAsync();
            var message = new CloudQueueMessage(JsonConvert.SerializeObject(queueMessage));
            await queue.AddMessageAsync(message);
        }

        await _repositoryFactory.SaveChangesAsync(cancellationToken);

        return true;

    }
}
