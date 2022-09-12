using AM180.Models.Enums;
using AM180.Models.Users;
using System.Text.Json.Serialization;
using System.Text.Json;
using AM180.Models.Abstractions;

namespace AM180.Converters;

public sealed class UserConverter : JsonConverter<User>
{
    public override bool CanConvert(Type typeToConvert) =>
        typeof(User).IsAssignableFrom(typeToConvert);

    public override User? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var clone = reader;
        while (clone.Read())
        {
            if (clone.TokenType == JsonTokenType.PropertyName)
            {
                var name = clone.GetString();
                if (name != null)
                {
                    if (name.ToLower() == nameof(UserType).ToLower())
                        break;
                }
            }
        }
        clone.Read();
        var type = (UserType)clone.GetInt32();
        return type switch
        {
            UserType.Default => JsonSerializer.Deserialize<DefaultUser>(ref reader, options),
            //UserType.Entertainer => JsonSerializer.Deserialize<EntertainerUser>(ref reader, options),
            _ => null,
        };
    }

    public override void Write(Utf8JsonWriter writer, User value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
}
