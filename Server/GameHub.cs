using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Treachery.Shared;

namespace Treachery.Server;

public partial class GameHub(DbContextOptions<TreacheryContext> dbContextOptions, IConfiguration configuration) : Hub<IGameClient>, IGameHub
{
    //State

    //Users
    private static ConcurrentDictionary<string,User> UsersByUserToken { get; } = [];
    private static ConcurrentDictionary<string,TokenInfo> UserTokenInfo { get; } = [];
    private static ConcurrentDictionary<int,UserConnections> ConnectionInfoByUserId { get; } = [];

    //Games
    private static ConcurrentDictionary<string,ManagedGame> GamesByGameId{ get; } = [];
    
    //Other
    private static DateTimeOffset MaintenanceDate { get; set; }
    private static DateTimeOffset LastCleanup { get; set; }
    
    private TreacheryContext GetDbContext() => new(dbContextOptions, configuration);

    private static string GenerateToken() => Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..16];
    
    private static VoidResult Error(ErrorType error, string errorDetails = "") => new() { Error = error, ErrorDetails = errorDetails, Success = false }; 
    
    private static Result<TResult> Error<TResult>(ErrorType error, string errorDetails = "") => new() { Error = error, ErrorDetails = errorDetails, Success = false }; 
    
    private static VoidResult Success() => new() { Success = true, Contents = new VoidContents()}; 
    
    private static Result<TResult> Success<TResult>(TResult contents) => new() { Success = true, Contents = contents };

    private static bool AreValid<TResult>(string userToken, string gameId, out User user, out ManagedGame game, out Result<TResult> result)
    {
        user = null;
        game = null;
        result = null;

        if (userToken == null)
            return false;
        
        if (!UsersByUserToken.TryGetValue(userToken, out user))
        {
            result = Error<TResult>(ErrorType.UserNotFound);
            return false;
        }
        
        if (gameId == null)
            return false;

        if (!GamesByGameId.TryGetValue(gameId, out game))
        {
            result = Error<TResult>(ErrorType.GameNotFound);
            return false;
        }

        return true;
    }    
    private static bool AreValid(string userToken, string gameId, out User user, out ManagedGame game, out VoidResult result)
    {
        user = null;
        game = null;
        result = null;
        
        if (userToken == null)
            return false;
        
        if (!UsersByUserToken.TryGetValue(userToken, out user))
        {
            result = Error(ErrorType.UserNotFound);
            return false;
        }
        
        if (gameId == null)
            return false;

        if (!GamesByGameId.TryGetValue(gameId, out game))
        {
            result = Error(ErrorType.GameNotFound);
            return false;
        }
        
        return true;
    }
    
    private void SendMail(MailMessage mail)
    {
        try
        {
            var username = configuration["GameEndEmailUsername"];
            if (string.IsNullOrEmpty(username)) 
                return;
            
            var password = configuration["GameEndEmailPassword"];

            SmtpClient client = new()
            {
                Credentials = new NetworkCredential(username, password),
                Host = "smtp.strato.com", //TODO: move to config
                EnableSsl = true
            };

            client.Send(mail);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error sending mail: {0}", e.Message);
        }
    }
    
    private static MemoryStream GenerateStreamFromString(string s)
    {
        var result = new MemoryStream();
        var writer = new StreamWriter(result);
        writer.Write(s);
        writer.Flush();
        result.Position = 0;
        return result;
    }
}