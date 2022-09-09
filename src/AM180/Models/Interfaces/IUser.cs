using AM180.Models.Enums;

namespace AM180.Models.Interfaces;

public interface IUser
{
    UserType UserType { get; set; }
}

