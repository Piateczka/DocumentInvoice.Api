using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentInvoice.Domain;

public class InvoiceItems
{
    public int Id { get; set; }
    public string Amount { get; set; }
    public int Quantity { get; set; }
    public string Tax { get; set; }
    public string TaxRate { get; set; }

    [ForeignKey("InvoiceId")]
    public Invoices Invoice { get; set; }

}
