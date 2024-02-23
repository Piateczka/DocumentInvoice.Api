using System.Runtime.Serialization;

namespace DocumentInvoice.Api.Helpers;

public static class Helpers
{
    public static T ParseEnum<T>(string value) where T : struct
    {
        if (!typeof(T).IsEnum)
            throw new ArgumentException("T must be an enumerated type");

        foreach (var field in typeof(T).GetEnumValues())
        {
            if (field.ToString() == value)
            {
                return (T)field;
            }

            var member = typeof(T).GetMember(field.ToString())[0];
            var attribute = (EnumMemberAttribute)Attribute.GetCustomAttribute(member, typeof(EnumMemberAttribute));
            if (attribute != null && attribute.Value == value)
            {
                return (T)field;
            }
        }

        throw new ArgumentException($"Value '{value}' was not found in enum {typeof(T).FullName}");
    }
}
