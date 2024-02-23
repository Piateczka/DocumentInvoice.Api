using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentInvoice.Domain;

public class Invoices
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; }
    public string CustomerName { get; set; }
    public string VendorName { get; set; }
    public ICollection<InvoiceItems> InvoiceItems { get; set; }

    [ForeignKey("DocumentId")]
    public Document Document { get; set; }
}
