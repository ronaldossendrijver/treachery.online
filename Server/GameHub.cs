using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Treachery.Server;

public partial class GameHub(DbContextOptions<TreacheryContext> dbContextOptions, IConfiguration configuration)
    : Hub<IGameClient>, IGameHub
{
    private const int CleanupFrequency = 3600000; // 3600000 ms = Each hour
    private const int ServerStatusFrequency = 6000; // 6000 ms = Each 6 seconds
    private const int PersistFrequency = 600000; // 600000 ms = Each 10 minutes
    private const int MaximumLoginTime = 10080; // 10080 minutes = 7 days 
    
    //Users
    private static ConcurrentDictionary<string,LoggedInUser> UsersByUserToken { get; } = [];
    private static ConcurrentDictionary<int,UserConnections> ConnectionInfoByUserId { get; } = [];

    //Started games
    private static ConcurrentDictionary<string,ManagedGame> RunningGamesByGameId{ get; } = [];
    
    //Scheduled games
    private static ConcurrentDictionary<string,ScheduledGame> ScheduledGamesByGameId{ get; } = [];

    private static ServerStatus ServerStatus { get; set; } = new();
    
    //Other
    private static DateTimeOffset LastRestored { get; set; }
    private static bool Restoring { get; set; }
    private static DateTimeOffset LastPersisted { get; set; }
    private static bool Persisting { get; set; }
    private static DateTimeOffset MaintenanceDate { get; set; }
    private static DateTimeOffset LastCleanedUp { get; set; }
    
    private IConfiguration Configuration { get; } = configuration;

    private DbContextOptions<TreacheryContext> DbContextOptions { get; set; } = dbContextOptions;

    private TreacheryContext GetDbContext() => new(DbContextOptions, Configuration);

    private static string GenerateToken() => Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..16];
    
    private static VoidResult Error(ErrorType error, string errorDetails = "") => new() { Error = error, ErrorDetails = errorDetails, Success = false }; 
    
    private static Result<TResult> Error<TResult>(ErrorType error, string errorDetails = "") => new() { Error = error, ErrorDetails = errorDetails, Success = false }; 
    
    private static VoidResult Success() => new() { Success = true, Contents = new VoidContents()}; 
    
    private static Result<TResult> Success<TResult>(TResult contents) => new() { Success = true, Contents = contents };

    private static bool AreValid<TResult>(string userToken, string gameId, out LoggedInUser user, out ManagedGame game, out Result<TResult> result)
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

        if (!RunningGamesByGameId.TryGetValue(gameId, out game))
        {
            result = Error<TResult>(ErrorType.GameNotFound);
            return false;
        }

        return true;
    }    
    private static bool AreValid(string userToken, string gameId, out LoggedInUser user, out ManagedGame game, out VoidResult result)
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

        if (!RunningGamesByGameId.TryGetValue(gameId, out game))
        {
            result = Error(ErrorType.GameNotFound);
            return false;
        }
        
        return true;
    }
    
    private async Task SendMail(MailMessage mail)
    {
        try
        {
            var username = Configuration["GameEndEmailUsername"];
            if (string.IsNullOrEmpty(username)) 
                return;
            
            var password = Configuration["GameEndEmailPassword"];

            SmtpClient client = new()
            {
                Credentials = new NetworkCredential(username, password),
                Host = "smtp.strato.com", //TODO: move to config
                EnableSsl = true
            };
            
            await client.SendMailAsync(mail);
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