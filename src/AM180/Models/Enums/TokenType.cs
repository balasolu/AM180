using System;
using System.ComponentModel;

namespace AM180.Models.Enums;

public enum TokenType
{
    [Description("Default")]
    Default,
    [Description("Authentication")]
    Authentication,
    [Description("Refresh")]
    Refresh,
    [Description("Confirmation")]
    Confirmation
}

