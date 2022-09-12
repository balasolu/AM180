using AM180.Converters;
using AM180.Models.Enums;
using AM180.Models.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace AM180.Models.Abstractions;

[JsonConverter(typeof(RoleConverter))]
public abstract class Role : IdentityRole, IEntity<string>, IRole
{
    public Role()
    {
    }

    public long? CreatedAt { get; set; }
    public RoleType RoleType { get; set; }
}

