using Azure.Search.Documents.Indexes;
using DocumentInvoice.Domain;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DocumentInvoice.Service.DTO;

public class DocumentSearch
{
    [JsonPropertyName("id")]
    [SimpleField]
    public string Id { get; set; }
    public int DocumentId { get; set; }
}
