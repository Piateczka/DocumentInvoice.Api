using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace DocumentInvoice.Service.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum DocumentStatus
{
    [EnumMember(Value = "New")]
    New = 0,
    [EnumMember(Value = "Valid")]
    Valid = 1,
    [EnumMember(Value = "Invalid")]
    Invalid = -1
}
