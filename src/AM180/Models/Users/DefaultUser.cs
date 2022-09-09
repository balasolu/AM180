using AM180.Models.Abstractions;
using AM180.Models.Interfaces;

namespace AM180.Models.Users;

public sealed class DefaultUser : User, IEntity<string>, IUser
{
    public DefaultUser()
    {
    }
}

