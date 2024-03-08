using Azure.Search.Documents.Indexes;
using DocumentInvoice.Domain;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DocumentInvoice.Service.DTO;

public class DocumentSearch
{
    [JsonPropertyName("DcoumentId")]
    public int Id { get; set; }
}
