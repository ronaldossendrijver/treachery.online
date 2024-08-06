using System;

namespace Treachery.Shared;

public class TokenInfo
{
    public DateTime Issued { get; } = DateTime.Now;
    
    public DateTime Refreshed { get; set; }
}