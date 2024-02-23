using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentInvoice.Domain;

public class Document
{
    public int Id { get; set; }
    public string DocumentName { get; set; }
    public string? DocumentFileName { get; set; }
    public int DocumentCategory { get; set; }
    public int DocumentStatus { get; set; }
    public DateTime UploadTime { get; set; }
    public string Url { get; set; }
    public string? Comment { get; set; }
    public string? Container { get; set; }
    public string? Month { get; set; }
    public string? Year { get; set; }
    [ForeignKey("Company")]
    public int CompanyId { get; set; }
    public Company Customer { get; set; }
}
