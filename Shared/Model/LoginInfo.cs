namespace Treachery.Shared;

public class LoginInfo
{
    public int UserId { get; init; }

    public string Token { get; init; } = string.Empty;
    
    public string PlayerName { get; init; } = string.Empty;
    
    public string UserName { get; init; } = string.Empty;
    
    public string Email { get; init; } = string.Empty;
}