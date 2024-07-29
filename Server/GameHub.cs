using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Treachery.Server.Data;
using Treachery.Shared;

namespace Treachery.Server;

public partial class GameHub(DbContextOptions<TreacheryContext> dbContextOptions, IConfiguration configuration) : Hub<IGameClient>, IGameHub
{
    private static ConcurrentDictionary<string,ManagedGame> GamesByGameToken { get; } = [];
    private static ConcurrentDictionary<string,string> GameTokensByGameId { get; } = [];
    private static ConcurrentDictionary<string,User> UsersByUserToken { get; } = [];
    private static ConcurrentDictionary<string,Game> FinishedGames { get; } = [];
    private static ConcurrentDictionary<string,DateTime> UserTokensLastSeen { get; } = [];

    private TreacheryContext GetDbContext() => new(dbContextOptions, configuration);

    private static string GenerateToken() => Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..10];
    
    private static VoidResult Error(string message) => new() { Message = message, Success = false }; 
    
    private static Result<TResult> Error<TResult>(string message) => new() { Message = message, Success = false }; 
    
    private static VoidResult Success() => new() { Success = true, Contents = new VoidContents()}; 
    
    private static Result<TResult> Success<TResult>(TResult contents) => new() { Success = true, Contents = contents };

    private static bool AreValid<TResult>(string playerToken, string gameToken, out User user, out ManagedGame game, out Result<TResult> result)
    {
        user = null;
        game = null;
        result = null;
        
        if (!UsersByUserToken.TryGetValue(playerToken, out user))
        {
            result = Error<TResult>("Player not found");
            return false;
        }

        if (!GamesByGameToken.TryGetValue(gameToken, out game))
        {
            result = Error<TResult>("Game not found");
            return false;
        }

        return true;
    }    
    private static bool AreValid(string playerToken, string gameToken, out User user, out ManagedGame game, out VoidResult result)
    {
        user = null;
        game = null;
        result = null;
        
        if (!UsersByUserToken.TryGetValue(playerToken, out user))
        {
            result = Error("Player not found");
            return false;
        }

        if (!GamesByGameToken.TryGetValue(gameToken, out game))
        {
            result = Error("Game not found");
            return false;
        }

        return true;
    }
    
    private void SendMail(MailMessage mail)
    {
        try
        {
            var username = configuration["GameEndEmailUsername"];
            if (username != "")
            {
                var password = configuration["GameEndEmailPassword"];

                SmtpClient client = new()
                {
                    Credentials = new NetworkCredential(username, password),
                    Host = "smtp.strato.com",
                    EnableSsl = true
                };

                client.Send(mail);
            }
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