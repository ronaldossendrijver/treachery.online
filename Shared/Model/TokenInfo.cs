using System;

namespace Treachery.Shared;

public class TokenInfo
{
    public DateTimeOffset Issued { get; } = DateTimeOffset.Now;
    
    public DateTimeOffset Refreshed { get; set; }
}