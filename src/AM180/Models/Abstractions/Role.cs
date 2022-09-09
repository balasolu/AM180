using AM180.Models.Enums;
using AM180.Models.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AM180.Models.Abstractions
{
    public abstract class Role : IdentityRole, IEntity<string>, IRole
    {
        public Role()
        {
        }

        public long? CreatedAt { get; set; }
        public RoleType RoleType { get; set; }
    }
}

