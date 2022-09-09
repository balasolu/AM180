using System;
using AM180.Models.Abstractions;
using AM180.Models.Interfaces;

namespace AM180.Models.Tokens;

public sealed class DefaultToken : Token, IEntity<string>, IToken
{
	public DefaultToken()
	{
	}
}

