using AM180.Converters;
using AM180.Models.Enums;
using AM180.Models.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace AM180.Models.Abstractions;

[JsonConverter(typeof(UserConverter))]
public abstract class User : IdentityUser, IEntity<string>, IUser
{
    public User()
    {
    }

    public long? CreatedAt { get; set; }
    public UserType UserType { get; set; }
    public virtual ICollection<Token>? Tokens { get; set; }
}

