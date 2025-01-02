namespace Treachery.Server;

public class LoggedInUser(User user)
{
    public User User => user;

    public int Id => User.Id;
    
    public string Name => User.Name;
    
    public string PlayerName => User.PlayerName;
    
    public string Email => User.Email;
    
    public DateTimeOffset LoggedInDateTime { get; } = DateTimeOffset.Now;

    public UserStatus Status { get; set; } = UserStatus.Online;
    
    public DateTimeOffset LastSeenDateTime { get; set; }
}