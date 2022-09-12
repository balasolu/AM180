using AM180.Models.Enums;
using AM180.Models.Roles;
using System.Text.Json.Serialization;
using System.Text.Json;
using AM180.Models.Abstractions;

namespace AM180.Converters;

public sealed class RoleConverter : JsonConverter<Role>
{
    public override bool CanConvert(Type typeToConvert) =>
        typeof(Role).IsAssignableFrom(typeToConvert);

    public override Role? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var clone = reader;
        while (clone.Read())
        {
            if (clone.TokenType == JsonTokenType.PropertyName)
            {
                var name = clone.GetString();
                if (name != null)
                {
                    if (name.ToLower() == nameof(RoleType).ToLower())
                        break;
                }
            }
        }
        clone.Read();
        var type = (RoleType)clone.GetInt32();
        return type switch
        {
            RoleType.Default => JsonSerializer.Deserialize<DefaultRole>(ref reader, options),
            //RoleType.Vendor => JsonSerializer.Deserialize<VendorRole>(ref reader, options),
            _ => null,
        };
    }

    public override void Write(Utf8JsonWriter writer, Role value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
}
