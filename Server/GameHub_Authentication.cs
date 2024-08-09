using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Treachery.Shared;

namespace Treachery.Server;

public partial class GameHub
{
    public async Task<Result<LoginInfo>> RequestCreateUser(string userName, string hashedPassword, string email, string playerName)
    {
        var trimmedUsername = userName.Trim().ToLower();
        
        if (trimmedUsername.Trim().Length <= 3)
        {
            return Error<LoginInfo>("Username must be more than 3 characters");
        }
        
        await using var db = GetDbContext();
        
        if (db.Users.Any(x => x.Name.Trim().ToLower().Equals(trimmedUsername)))
        {
            return Error<LoginInfo>("This username already exists");
        }
        
        if (db.Users.Any(x => x.Email.Trim().ToLower().Equals(trimmedUsername)))
        {
            return Error<LoginInfo>("This e-mail address is already in use");
        }

        db.Add(new User
            { Name = trimmedUsername, HashedPassword = hashedPassword, Email = email, PlayerName = playerName });

        await db.SaveChangesAsync();
        
        var user = await db.Users.FirstOrDefaultAsync(x =>
            x.Name.Trim().ToLower().Equals(userName.Trim().ToLower()) && x.HashedPassword == hashedPassword);
        
        if (user == null)
            return Error<LoginInfo>("User creation failed");
        
        MailMessage mailMessage = new()
        {
            From = new MailAddress("noreply@treachery.online"),
            Subject = "Welcome to treachery.online",
            IsBodyHtml = true,
            Body = $"""
                    Welcome to treachery.online, {trimmedUsername}!
                    {Environment.NewLine}{Environment.NewLine}
                    If you ever need to reset your password, a reset token will be sent to this e-mail address.
                    """
        };

        mailMessage.To.Add(new MailAddress(email));
        SendMail(mailMessage);
        
        var userToken = GenerateToken();
        UsersByUserToken.TryAdd(userToken, user);
        UserTokenInfo[userToken] = new TokenInfo();
        return Success(new LoginInfo { UserId = user.Id, Token = userToken, PlayerName = user.PlayerName, UserName = user.Name, Email = user.Email });
    }
    
    public async Task<Result<LoginInfo>> RequestLogin(int version, string userName, string hashedPassword)
    {
        if (Game.LatestVersion != version)
            return Error<LoginInfo>("Invalid game version");
        
        await using var db = GetDbContext();
        
        var user = await db.Users.FirstOrDefaultAsync(x =>
            x.Name.Trim().ToLower().Equals(userName.Trim().ToLower()) && x.HashedPassword == hashedPassword);
        
        if (user == null)
            return Error<LoginInfo>("Invalid user name or password");

        var userToken = GenerateToken();
        UsersByUserToken.TryAdd(userToken, user);
        UserTokenInfo[userToken] = new TokenInfo();
        return Success(new LoginInfo { UserId = user.Id, Token = userToken, PlayerName = user.PlayerName, UserName = user.Name, Email = user.Email });
    }
    
    public async Task<VoidResult> RequestPasswordReset(string email)
    {
        await using var db = GetDbContext();
        
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email.Trim().ToLower().Equals(email.Trim().ToLower()));
        
        if (user == null)
            return Error("Unknown email address");

        if ((DateTime.Now - user.PasswordResetTokenCreated).TotalMinutes < 10)
            return Error("Please wait at least 10 minutes before requesting another password reset");

        var token = GenerateToken();
        user.PasswordResetToken = token;
        user.PasswordResetTokenCreated = DateTime.Now;

        await db.SaveChangesAsync();

        MailMessage mailMessage = new()
        {
            From = new MailAddress("noreply@treachery.online"),
            Subject = "Password Reset",
            IsBodyHtml = true,
            Body = $"""
                    You have requested a password reset for user: {user.Name}
                    {Environment.NewLine}{Environment.NewLine}
                    You can use this token to reset your password: {token}
                    """
        };

        mailMessage.To.Add(new MailAddress(user.Email));
        SendMail(mailMessage);
        
        return null;
    }
    
    public async Task<Result<LoginInfo>> RequestSetPassword(string userName, string passwordResetToken, string newHashedPassword)
    {
        await using var db = GetDbContext();

        var user = await db.Users.FirstOrDefaultAsync(x =>
            x.Name.Trim().ToLower().Equals(userName.Trim().ToLower()));
        
        if (user == null)
            return Error<LoginInfo>("Unknown user name");

        if (string.IsNullOrEmpty(user.PasswordResetToken) || user.PasswordResetToken.Trim() != passwordResetToken)
            return Error<LoginInfo>("Invalid password reset token");
        
        if ((DateTime.Now - user.PasswordResetTokenCreated).TotalMinutes > 60)
            return Error<LoginInfo>("Your password reset token has expired");

        user.HashedPassword = newHashedPassword;
        user.PasswordResetToken = null;
        user.PasswordResetTokenCreated = default;

        await db.SaveChangesAsync();
        
        var userToken = GenerateToken();
        UsersByUserToken.TryAdd(userToken, user);
        return Success(new LoginInfo { UserId = user.Id, Token = userToken, PlayerName = user.PlayerName, UserName = user.Name, Email = user.Email });
    }
    
    public async Task<Result<LoginInfo>> GetLoginInfo(string userToken)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user))
            return Error<LoginInfo>("User not found");
            
        return await Task.FromResult(Success(new LoginInfo { UserId = user.Id, Token = userToken, PlayerName = user.PlayerName, UserName = user.Name, Email = user.Email }));
    }
    
    public async Task<Result<LoginInfo>> RequestUpdateUserInfo(string userToken, string hashedPassword, string playerName, string email)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user))
            return Error<LoginInfo>("User not found");
        
        await using var db = GetDbContext();

        var dbUser = await db.Users.FindAsync(user.Id); 
        
        if (dbUser == null)
            return Error<LoginInfo>("User not found");

        if (!string.IsNullOrEmpty(hashedPassword))
            dbUser.HashedPassword = hashedPassword;
        
        dbUser.PlayerName = playerName;
        dbUser.Email = email;

        await db.SaveChangesAsync();
            
        return await Task.FromResult(Success(new LoginInfo { UserId = dbUser.Id, Token = userToken, PlayerName = dbUser.PlayerName, UserName = dbUser.Name, Email = dbUser.Email }));
    }

}

