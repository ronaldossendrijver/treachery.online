using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Treachery.Server;

public partial class GameHub
{
    public async Task<Result<LoginInfo>> RequestCreateUser(string userName, string hashedPassword, string email, string playerName)
    {
        var cleanedUsername = userName.Trim().ToLower();
        
        if (cleanedUsername.Length <= 3)
        {
            return Error<LoginInfo>(ErrorType.UserNameTooShort);
        }
        
        if (cleanedUsername.Length > 40)
        {
            return Error<LoginInfo>(ErrorType.UserNameTooLong);
        }
        
        var trimmedPlayerName = playerName.Trim();
        
        if (trimmedPlayerName.Length <= 3)
        {
            return Error<LoginInfo>(ErrorType.PlayerNameTooShort);
        }
        
        if (trimmedPlayerName.Length > 40)
        {
            return Error<LoginInfo>(ErrorType.PlayerNameTooLong);
        }
        
        await using var db = GetDbContext();
        
        if (db.Users.Any(x => x.Name.Equals(cleanedUsername)))
        {
            return Error<LoginInfo>(ErrorType.UserNameExists);
        }

        db.Add(new User
            { Name = cleanedUsername, HashedPassword = hashedPassword, Email = email, PlayerName = trimmedPlayerName, LastLogin = DateTimeOffset.Now });

        await db.SaveChangesAsync();
        
        var user = await db.Users.FirstOrDefaultAsync(x =>
            x.Name.Equals(cleanedUsername) && x.HashedPassword == hashedPassword);

        if (user == null)
            return Error<LoginInfo>(ErrorType.UserCreationFailed);

        UsersById.TryAdd(user.Id, user);
        
        MailMessage mailMessage = new()
        {
            From = new MailAddress("admin@treachery.online"),
            Subject = "Welcome to treachery.online",
            IsBodyHtml = true,
            Body = $"""
                    <p>Welcome to treachery.online, <strong>{cleanedUsername}</strong>.</p>
                    <p>Your player name is: <strong>{trimmedPlayerName}</strong>.</p>
                    <p>If you ever need to reset your password, a reset token will be sent to this e-mail address.</p>
                    <p>You can edit your player name, email address and password any time after logging in.</p>
                    """
        };

        var userToken = LoginAndCreateToken(user);
        
        mailMessage.To.Add(new MailAddress(email));
        await SendMail(mailMessage);
        
        return Success(new LoginInfo { UserId = user.Id, Token = userToken, PlayerName = user.PlayerName, UserName = user.Name, Email = user.Email });
    }

    private static string LoginAndCreateToken(User user)
    {
        var existingLoggedInUser = UsersByUserToken.FirstOrDefault(x => x.Value.Id == user.Id);
        if (existingLoggedInUser.Value != null && existingLoggedInUser.Value.Id == user.Id)
            return existingLoggedInUser.Key;
        
        var userToken = GenerateToken();
        var loggedInUser = new LoggedInUser(user);
        UsersByUserToken.TryAdd(userToken, loggedInUser);
        return userToken;
    } 
    
    public async Task<Result<LoginInfo>> RequestLogin(int version, string userName, string hashedPassword)
    {
        if (Game.LatestVersion != version)
            return Error<LoginInfo>(ErrorType.InvalidGameVersion);
        
        await using var db = GetDbContext();
      
        var user = await db.Users.FirstOrDefaultAsync(x =>
            x.Name.Trim().ToLower().Equals(userName.Trim().ToLower()) && x.HashedPassword == hashedPassword);
        
        if (user == null)
            return Error<LoginInfo>(ErrorType.InvalidUserNameOrPassword);
        
        user.LastLogin = DateTimeOffset.Now;
        await db.SaveChangesAsync();

        var userToken = LoginAndCreateToken(user);
        
        return Success(new LoginInfo { UserId = user.Id, Token = userToken, PlayerName = user.PlayerName, UserName = user.Name, Email = user.Email });
    }
    
    public async Task<VoidResult> RequestPasswordReset(string usernameOrEmail)
    {
        await using var db = GetDbContext();
        
        var users = db.Users.Where(x => 
            x.Email.Trim().ToLower().Equals(usernameOrEmail.Trim().ToLower()) ||
            x.Name.Trim().ToLower().Equals(usernameOrEmail.Trim().ToLower()))
            .ToList();
        
        if (users.Count == 0)
            return Error(ErrorType.UnknownUsernameOrEmailAddress);

        if (users.Any(u => (DateTimeOffset.Now - u.PasswordResetTokenCreated).TotalMinutes < 5))
            return Error(ErrorType.ResetRequestTooSoon);

        var token = GenerateToken();
        var now = DateTimeOffset.Now;

        foreach (var user in users)
        {
            user.PasswordResetToken = token;
            user.PasswordResetTokenCreated = now; 
        }

        await db.SaveChangesAsync();

        var usersMessage = users.Count == 1 ? 
            "user: " + users[0].Name : 
            "users: " + string.Join(", ", users.Select(u => u.Name));
        
        MailMessage mailMessage = new()
        {
            From = new MailAddress("noreply@treachery.online"),
            Subject = "Password Reset",
            IsBodyHtml = true,
            Body = $"""
                    <p>You have requested a password reset for <strong>{usersMessage}</strong>.</p>
                    <p>You can use this token to reset your password: <strong>{token}</strong>.</p>
                    """
        };

        mailMessage.To.Add(new MailAddress(users[0].Email));
        await SendMail(mailMessage);
        
        return Success();
    }
    
    public async Task<Result<LoginInfo>> RequestSetPassword(string userName, string passwordResetToken, string newHashedPassword)
    {
        await using var db = GetDbContext();

        var user = await db.Users.FirstOrDefaultAsync(x =>
            x.Name.Trim().ToLower().Equals(userName.Trim().ToLower()));
        
        if (user == null)
            return Error<LoginInfo>(ErrorType.UnknownUserName);

        if (string.IsNullOrEmpty(user.PasswordResetToken) || user.PasswordResetToken.Trim() != passwordResetToken)
            return Error<LoginInfo>(ErrorType.InvalidResetToken);
        
        if ((DateTime.Now - user.PasswordResetTokenCreated).TotalMinutes > 60)
            return Error<LoginInfo>(ErrorType.ResetTokenExpired);

        user.HashedPassword = newHashedPassword;
        user.PasswordResetToken = string.Empty;
        user.PasswordResetTokenCreated = default;

        await db.SaveChangesAsync();
        
        var userToken = LoginAndCreateToken(user);
        return Success(new LoginInfo { UserId = user.Id, Token = userToken, PlayerName = user.PlayerName, UserName = user.Name, Email = user.Email });
    }
    
    public async Task<Result<LoginInfo>> GetLoginInfo(string userToken)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user))
            return Error<LoginInfo>(ErrorType.UserNotFound);
            
        return await Task.FromResult(Success(new LoginInfo { UserId = user.Id, Token = userToken, PlayerName = user.PlayerName, UserName = user.Username, Email = user.Email }));
    }
    
    public async Task<Result<LoginInfo>> RequestUpdateUserInfo(string userToken, string hashedPassword, string playerName, string email)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var loggedInUser))
            return Error<LoginInfo>(ErrorType.UserNotFound);

        var user = loggedInUser.User;
        user.PlayerName = playerName.Trim();
        user.Email = email;
        if (!string.IsNullOrEmpty(hashedPassword))
            user.HashedPassword = hashedPassword;
        
        await using var db = GetDbContext();
        db.Users.Update(user);
        await db.SaveChangesAsync();
        
        UsersById[user.Id] = user;
            
        return await Task.FromResult(Success(new LoginInfo { UserId = user.Id, Token = userToken, PlayerName = user.PlayerName, UserName = user.Name, Email = user.Email }));
    }

    public async Task<Result<ServerStatus>> RequestSetUserStatus(string userToken, UserStatus status)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user))
            return Error<ServerStatus>(ErrorType.UserNotFound);

        user.Status = status;
        UpdateServerStatusIfNeeded(true);
        await Task.CompletedTask;
        
        return Success(FilteredServerStatus(GameListScope.Active, user.Id));
    }
}

