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
    private readonly ConcurrentDictionary<string,ManagedGame> gamesByGameToken = [];
    private readonly ConcurrentDictionary<string,string> gameTokensByGameId = [];
    private readonly ConcurrentDictionary<string,User> usersByUserToken = [];
    private readonly ConcurrentDictionary<string,Game> finishedGames = [];
    private readonly ConcurrentDictionary<string,DateTime> userTokensLastSeen = [];
    //private readonly ConcurrentDictionary<int, Game> gamesByPlayerId = [];

    

    private TreacheryContext GetDbContext() => new(dbContextOptions, configuration);

    private string GenerateToken() => Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..10];
    
    private VoidResult Error(string message) => new() { Message = message, Success = false }; 
    
    private Result<TResult> Error<TResult>(string message) => new() { Message = message, Success = false }; 
    
    private VoidResult Success() => new() { Success = true, Contents = new VoidContents()}; 
    
    private Result<TResult> Success<TResult>(TResult contents) => new() { Success = true, Contents = contents };

    private bool AreValid<TResult>(string playerToken, string gameToken, out User user, out ManagedGame game, out Result<TResult> result)
    {
        user = null;
        game = null;
        result = null;
        
        if (!usersByUserToken.TryGetValue(playerToken, out user))
        {
            result = Error<TResult>("Player not found");
            return false;
        }

        if (!gamesByGameToken.TryGetValue(gameToken, out game))
        {
            result = Error<TResult>("Game not found");
            return false;
        }

        return true;
    }    
    private bool AreValid(string playerToken, string gameToken, out User user, out ManagedGame game, out VoidResult result)
    {
        user = null;
        game = null;
        result = null;
        
        if (!usersByUserToken.TryGetValue(playerToken, out user))
        {
            result = Error("Player not found");
            return false;
        }

        if (!gamesByGameToken.TryGetValue(gameToken, out game))
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
    
    private static Stream GenerateStreamFromString(string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}