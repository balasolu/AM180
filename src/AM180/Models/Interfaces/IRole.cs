using System;
using AM180.Models.Enums;

namespace AM180.Models.Interfaces;

public interface IRole
{
    RoleType RoleType { get; set; }
}

