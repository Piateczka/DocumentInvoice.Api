using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace DocumentInvoice.Service.Enums;


[JsonConverter(typeof(StringEnumConverter))]
public enum UserRole
{
    [EnumMember(Value = "Admin")]
    Admin = 1,
    [EnumMember(Value = "Accountant")]
    Accountant = 2,
    [EnumMember(Value = "User")]
    User = 3

}
