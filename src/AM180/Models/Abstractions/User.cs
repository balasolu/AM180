using System;
using AM180.Models.Enums;
using AM180.Models.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AM180.Models.Abstractions;

public abstract class User : IdentityUser, IEntity<string>, IUser
{
	public User()
	{
	}

    public long? CreatedAt { get; set; }
    public UserType UserType { get; set; }
}

