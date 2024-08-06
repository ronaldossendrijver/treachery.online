using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Treachery.Server.Data;
using Treachery.Shared;

namespace Treachery.Server;

public partial class GameHub(DbContextOptions<TreacheryContext> dbContextOptions, IConfiguration configuration) : Hub<IGameClient>, IGameHub
{
    private static ConcurrentDictionary<string,string> GameTokensByGameId { get; } = [];
    private static ConcurrentDictionary<string,ManagedGame> GamesByGameToken { get; } = [];
    private static ConcurrentDictionary<string,TokenInfo> GameTokenInfo { get; } = [];
    private static ConcurrentDictionary<string,Game> FinishedGames { get; } = [];
    
    private static ConcurrentDictionary<string,User> UsersByUserToken { get; } = [];
    private static ConcurrentDictionary<string,TokenInfo> UserTokenInfo { get; } = [];
    private static ConcurrentDictionary<int,ConnectionInfo> ConnectionInfoByUserId { get; } = [];
    
    private static DateTime MaintenanceDate { get; set; }
    
    private TreacheryContext GetDbContext() => new(dbContextOptions, configuration);

    private static string GenerateToken() => Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..16];
    
    private static VoidResult Error(string message) => new() { Message = message, Success = false }; 
    
    private static Result<TResult> Error<TResult>(string message) => new() { Message = message, Success = false }; 
    
    private static VoidResult Success() => new() { Success = true, Contents = new VoidContents()}; 
    
    private static Result<TResult> Success<TResult>(TResult contents) => new() { Success = true, Contents = contents };

    private static bool AreValid<TResult>(string userToken, string gameToken, out User user, out ManagedGame game, out Result<TResult> result)
    {
        user = null;
        game = null;
        result = null;

        if (userToken == null)
            return false;
        
        if (!UsersByUserToken.TryGetValue(userToken, out user))
        {
            result = Error<TResult>("User not found");
            return false;
        }
        
        if (gameToken == null)
            return false;

        if (!GamesByGameToken.TryGetValue(gameToken, out game))
        {
            result = Error<TResult>("Game not found");
            return false;
        }
        
        if (!game.Game.IsParticipant(user.Id))
        {
            result = Error<TResult>("User not found");
            return false;
        }

        return true;
    }    
    private static bool AreValid(string userToken, string gameToken, out User user, out ManagedGame game, out VoidResult result)
    {
        user = null;
        game = null;
        result = null;
        
        if (userToken == null)
            return false;
        
        if (!UsersByUserToken.TryGetValue(userToken, out user))
        {
            result = Error("User not found");
            return false;
        }
        
        if (gameToken == null)
            return false;

        if (!GamesByGameToken.TryGetValue(gameToken, out game))
        {
            result = Error("Game not found");
            return false;
        }

        if (!game.Game.IsParticipant(user.Id))
        {
            result = Error("User not found");
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