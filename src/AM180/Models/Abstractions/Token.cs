﻿using AM180.Converters;
using AM180.Models.Enums;
using AM180.Models.Interfaces;
using System.Text.Json.Serialization;

namespace AM180.Models.Abstractions;

[JsonConverter(typeof(TokenConverter))]
public abstract class Token : Entity<string>, IEntity<string>, IToken
{
    public Token()
    {
    }

    public TokenType TokenType { get; set; }
    public long? Expiration { get; set; }
    public string? Hash { get; set; }
    public virtual string? UserForeignKey { get; set; }
}

