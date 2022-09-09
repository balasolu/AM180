using AM180.Models.Abstractions;
using AM180.Models.Interfaces;

namespace AM180.Models.Tokens;

public sealed class RefreshToken : Token, IEntity<string>, IToken
{
    public RefreshToken()
    {
    }
}

