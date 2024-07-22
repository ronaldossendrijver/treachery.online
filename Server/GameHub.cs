using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Treachery.Client;
using Treachery.Server.Data;
using Treachery.Shared;

namespace Treachery.Server;

public partial class GameHub(DbContextOptions<TreacheryContext> dbContextOptions, IConfiguration configuration) : Hub<IGameClient>, IGameHub
{
    private readonly ConcurrentDictionary<string,ManagedGame> gamesByToken = [];
    private readonly ConcurrentDictionary<Guid,string> gameTokensByGameId = [];
    private readonly ConcurrentDictionary<string,int> playerIdsByToken = [];
    private readonly ConcurrentDictionary<string,Game> finishedGames = [];
    private readonly ConcurrentDictionary<string,DateTime> playerTokensLastSeen = [];
    //private readonly ConcurrentDictionary<int, Game> gamesByPlayerId = [];

    

    private TreacheryContext GetDbContext() => new(dbContextOptions, configuration);

    private string GenerateToken() => Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..10];
    
    private VoidResult Error(string message) => new() { Message = message, Success = false }; 
    
    private Result<TResult> Error<TResult>(string message) => new() { Message = message, Success = false }; 
    
    private VoidResult Success() => new() { Success = true, Contents = new VoidContents()}; 
    
    private Result<TResult> Success<TResult>(TResult contents) => new() { Success = true, Contents = contents };

    private bool AreValid<TResult>(string playerToken, string gameToken, out int playerId, out ManagedGame game, out Result<TResult> result)
    {
        playerId = -1;
        game = null;
        result = null;
        
        if (!playerIdsByToken.TryGetValue(playerToken, out playerId))
        {
            result = Error<TResult>("Player not found");
            return false;
        }

        if (!gamesByToken.TryGetValue(gameToken, out game))
        {
            result = Error<TResult>("Game not found");
            return false;
        }

        return true;
    }    
    private bool AreValid(string playerToken, string gameToken, out int playerId, out ManagedGame game, out VoidResult result)
    {
        playerId = -1;
        game = null;
        result = null;
        
        if (!playerIdsByToken.TryGetValue(playerToken, out playerId))
        {
            result = Error("Player not found");
            return false;
        }

        if (!gamesByToken.TryGetValue(gameToken, out game))
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