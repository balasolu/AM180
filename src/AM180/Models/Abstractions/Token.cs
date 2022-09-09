using AM180.Models.Enums;
using AM180.Models.Interfaces;

namespace AM180.Models.Abstractions
{
    public abstract class Token : Entity<string>, IEntity<string>, IToken
    {
        public Token()
        {
        }

        public TokenType TokenType { get; set; }
    }
}

