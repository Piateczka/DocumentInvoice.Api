using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace DocumentInvoice.Service.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum DocumentCategory
{
    [EnumMember(Value = "Vat Invoice")]
    VatInvoice,
    [EnumMember(Value = "Prepayment Invoice")]
    PrepaymentInvoice,
    [EnumMember(Value = "Proforma Invoice")]
    ProformaInvoice,
    [EnumMember(Value = "Correction Invoice")]
    CorrectionInvoice,
    [EnumMember(Value = "Other")]
    Other

}
