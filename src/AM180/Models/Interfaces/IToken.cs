﻿using AM180.Models.Enums;

namespace AM180.Models.Interfaces;

public interface IToken
{
    TokenType TokenType { get; set; }
    long? Expiration { get; set; }
    string? Hash { get; set; }
}

