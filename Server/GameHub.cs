namespace Treachery.Server;

public partial class GameHub(DbContextOptions<TreacheryContext> dbContextOptions, IConfiguration configuration)
    : Hub<IGameClient>, IGameHub
{
    private const int CleanupFrequencyHours = 7;
    private const int ServerStatusFrequencyMs = 6000;
    private const int PersistFrequencyMinutes = 30;
    private const int MaximumLoginTimeDays = 90; 
    private const int ActiveGameThresholdMinutes = 120;
    private const int GamePersistFrequencyMinutes = 5;
    private const int MaximumNumberOfGamesPerPlayer = 10;
    
    //Users
    private static ConcurrentDictionary<string,LoggedInUser> UsersByUserToken { get; } = [];
    private static ConcurrentDictionary<int, User> UsersById { get; } = [];
    private static ConcurrentDictionary<int,UserConnections> ConnectionInfoByUserId { get; } = [];

    //Started games
    private static ConcurrentDictionary<string,ManagedGame> RunningGamesByGameId{ get; } = [];
    
    //Scheduled games
    private static ConcurrentDictionary<string,ScheduledGame> ScheduledGamesByGameId{ get; } = [];

    //Server status
    private static GameInfo[] RunningGames { get; set; } = [];
    private static ScheduledGameInfo[] ScheduledGames { get; set; } = [];
    private static LoggedInUserInfo[] RecentlySeenUsers { get; set; } = [];
    
    //Other
    private static DateTimeOffset LastRestored { get; set; }
    private static DateTimeOffset LastPersistedScheduledGames { get; set; }
    private static DateTimeOffset MaintenanceDate { get; set; }
    private static DateTimeOffset LastCleanedUpUserTokens { get; set; }
    private static DateTimeOffset LastCleanedUpScheduledGames { get; set; }
    
    private static DateTimeOffset LastUpdatedServerStatus { get; set; }
    
    private IConfiguration Configuration { get; } = configuration;

    private DbContextOptions<TreacheryContext> DbContextOptions { get; } = dbContextOptions;

    private TreacheryContext GetDbContext() => new(DbContextOptions, Configuration);

    
    private static readonly char[] TokenChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
    private static readonly Random TokenRandom = new();
    private static string GenerateToken() 
    {
        var result = new char[16];
        for (var i = 0; i < 16; i++)
        {
            result[i] = TokenChars[TokenRandom.Next(TokenChars.Length)];
        }
        return new string(result);
    }
    
    private static VoidResult Error(ErrorType error, string errorDetails = "") 
        => new() { Error = error, ErrorDetails = errorDetails, Success = false }; 
    
    private static Result<TResult> Error<TResult>(ErrorType error, string errorDetails = "") 
        => new() { Error = error, ErrorDetails = errorDetails, Success = false }; 
    
    private static VoidResult Success() => new() { Success = true, Contents = new VoidContents()}; 
    
    private static Result<TResult> Success<TResult>(TResult contents) => new() { Success = true, Contents = contents };

    private static bool AreValid<TResult>(string? userToken, string? gameId, out LoggedInUser? user, out ManagedGame? game, out Result<TResult>? error)
    {
        user = null;
        game = null;
        error = null;

        if (userToken == null)
        {
            error = Error<TResult>(ErrorType.UserTokenNotFound);
            return false;
        }

        if (!UsersByUserToken.TryGetValue(userToken, out user))
        {
            error = Error<TResult>(ErrorType.UserNotFound);
            return false;
        }

        if (gameId == null)
        {
            error = Error<TResult>(ErrorType.GameIdNotFound);
            return false;
        }
            
        if (RunningGamesByGameId.TryGetValue(gameId, out game)) return true;
        
        error = Error<TResult>(ErrorType.GameNotFound);
        return false;
    }
    
    private static bool AreValid(string? userToken, string? gameId, out LoggedInUser? user, out ManagedGame? game, out VoidResult? error)
    {
        user = null;
        game = null;
        error = Success();
        
        if (userToken == null)
        {
            error = Error(ErrorType.UserTokenNotFound);
            return false;
        }

        if (!UsersByUserToken.TryGetValue(userToken, out user))
        {
            error = Error(ErrorType.UserNotFound);
            return false;
        }
        
        if (gameId == null)
        {
            error = Error(ErrorType.GameIdNotFound);
            return false;
        }

        if (RunningGamesByGameId.TryGetValue(gameId, out game)) return true;
        
        error = Error(ErrorType.GameNotFound);
        return false;
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