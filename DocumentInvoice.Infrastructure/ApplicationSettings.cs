namespace DocumentInvoice.Infrastructure;

public class ApplicationSettings
{
    public string BlobConnectionString { get; set; }
    public string DbConnectionString { get; set; }
    public string StorageAccountAccessKey { get; set; }
    public string ContainerName { get; set; }
    public string FormRecognizerKey { get; set; }
    public string FormRecognizerEndpoint { get; set; }
}
