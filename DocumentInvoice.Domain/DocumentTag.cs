using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentInvoice.Domain;

public class DocumentTag
{
    public int Id { get; set; }
    public string Tag { get; set; }
    public bool IsActive { get; set; }
    [ForeignKey("DocumentId")]
    public Document Document { get; set; }
}
