using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Storage.Blobs;
using DocumentInvoice.Domain;
using DocumentInvoice.Infrastructure;
using DocumentInvoice.Infrastructure.Repository;
using DocumentInvoice.Service.Command;
using MediatR;
using Microsoft.Extensions.Options;

namespace DocumentInvoice.Service.Handlers.Command;

public class AnalysisDocumentCommandHandler : IRequestHandler<AnalysisDocumentCommand, Unit>
{
    private readonly IRepositoryFactory<DocumentInvoiceContext> _repositoryFactory;
    private readonly IRepository<Invoices> _invoicesRepo;
    private readonly IRepository<InvoiceItems> _invoiceItemsRepo;
    private readonly IRepository<Document> _documentRepo;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ApplicationSettings _configuration;

    public AnalysisDocumentCommandHandler(IRepositoryFactory<DocumentInvoiceContext> repositoryFactory, BlobServiceClient blobServiceClient, IOptions<ApplicationSettings> configuration)
    {
        _repositoryFactory = repositoryFactory;
        _invoicesRepo = _repositoryFactory.GetRepository<Invoices>();
        _documentRepo = _repositoryFactory.GetRepository<Document>();
        _blobServiceClient = blobServiceClient;
        _configuration = configuration.Value;
    }

    public async Task<Unit> Handle(AnalysisDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = _documentRepo.Query.Single(x => x.Id == request.DocumentId);
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(document.Container);

        BlobClient blobClient = containerClient.GetBlobClient(request.DocumentName);

        using (MemoryStream stream = new MemoryStream())
        {
            await blobClient.DownloadToAsync(stream);
            stream.Position = 0;

            var formRecognizerClient = new DocumentAnalysisClient(new Uri(_configuration.FormRecognizerEndpoint), new AzureKeyCredential(_configuration.FormRecognizerKey));
            var analyzeResult = await formRecognizerClient.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-invoice", stream);
            AnalyzeResult result = analyzeResult.Value;
            Invoices invoice = new Invoices();
            invoice.Document = document;
            AnalyzedDocument analyzed = result.Documents[0];

            if (analyzed.Fields.TryGetValue("VendorName", out DocumentField vendorNameField))
            {
                if (vendorNameField.FieldType == DocumentFieldType.String)
                {
                    invoice.VendorName = vendorNameField.Value.AsString();
                }
            }

            if (analyzed.Fields.TryGetValue("CustomerName", out DocumentField customerNameField))
            {
                if (customerNameField.FieldType == DocumentFieldType.String)
                {
                    invoice.CustomerName = customerNameField.Value.AsString();
                }
            }

            if (analyzed.Fields.TryGetValue("InvoiceId", out DocumentField customerInvoiceIdField))
            {
                if (customerInvoiceIdField.FieldType == DocumentFieldType.String)
                {
                    invoice.InvoiceNumber = customerInvoiceIdField.Value.AsString();
                }
            }
            await _invoicesRepo.AddAsync(invoice, cancellationToken);
            await _repositoryFactory.SaveChangesAsync(cancellationToken);

            for (int i = 0; i < result.Documents.Count; i++)
            {
                AnalyzedDocument analyzedDocument = result.Documents[i];

                if (analyzedDocument.Fields.TryGetValue("Items", out DocumentField itemsField))
                {

                    if (itemsField.FieldType == DocumentFieldType.List)
                    {
                        foreach (DocumentField itemField in itemsField.Value.AsList())
                        {
                            InvoiceItems invoiceItem = new InvoiceItems();
                            invoiceItem.Invoice = invoice;

                            if (itemField.FieldType == DocumentFieldType.Dictionary)
                            {
                                IReadOnlyDictionary<string, DocumentField> itemFields = itemField.Value.AsDictionary();


                                if (itemFields.TryGetValue("TaxRate", out DocumentField itemTaxRateField))
                                {
                                    if (itemTaxRateField.FieldType == DocumentFieldType.String)
                                    {
                                        invoiceItem.TaxRate = itemTaxRateField.Value.AsString();
                                    }

                                }

                                if (itemFields.TryGetValue("Amount", out DocumentField itemAmountField))
                                {
                                    if (itemAmountField.FieldType == DocumentFieldType.Currency)
                                    {
                                        invoiceItem.Amount = itemAmountField.Value.AsCurrency().Amount.ToString();
                                    }
                                }

                                if (itemFields.TryGetValue("Quantity", out DocumentField itemQuantityField))
                                {
                                    if (itemQuantityField.FieldType == DocumentFieldType.Double)
                                    {
                                        invoiceItem.Quantity = (int)itemQuantityField.Value.AsDouble();
                                    }
                                }

                                if (itemFields.TryGetValue("Tax", out DocumentField itemTaxField))
                                {
                                    if (itemTaxField.FieldType == DocumentFieldType.Currency)
                                    {
                                        invoiceItem.Tax = itemTaxField.Value.AsCurrency().Amount.ToString();
                                    }
                                }
                            }
                            await _invoiceItemsRepo.AddAsync(invoiceItem, cancellationToken);
                            await _repositoryFactory.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
            }



        }

        return Unit.Value;
    }
}
