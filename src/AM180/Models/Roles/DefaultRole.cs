using System;
using AM180.Models.Abstractions;
using AM180.Models.Interfaces;

namespace AM180.Models.Roles;

public sealed class DefaultRole : Role, IEntity<string>, IRole
{
	public DefaultRole()
	{
	}
}

