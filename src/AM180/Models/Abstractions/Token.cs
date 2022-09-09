using AM180.Models.Enums;
using AM180.Models.Interfaces;

namespace AM180.Models.Abstractions;

public abstract class Token : Entity<string>, IEntity<string>, IToken
{
    public Token()
    {
    }

    public TokenType TokenType { get; set; }
    public long? Expiration { get; set; }
    public string? Hash { get; set; }
    public virtual string? UserId { get; set; }
}

