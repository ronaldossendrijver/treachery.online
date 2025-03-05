namespace Treachery.Server;

public partial class GameHub(DbContextOptions<TreacheryContext> dbContextOptions, IConfiguration configuration)
    : Hub<IGameClient>, IGameHub
{
    private const int CleanupFrequencyHours = 7;
    private const int ServerStatusFrequencyMs = 6000;
    private const int PersistFrequencyMinutes = 20;
    private const int MaximumLoginTimeDays = 90; 
    private const int ActiveGameThresholdMinutes = 30;
    private const int GamePersistFrequencyMinutes = 5;
    
    //Users
    private static ConcurrentDictionary<string,LoggedInUser> UsersByUserToken { get; } = [];
    private static ConcurrentDictionary<int, User> UsersById { get; } = [];
    private static ConcurrentDictionary<int,UserConnections> ConnectionInfoByUserId { get; } = [];

    //Started games
    private static ConcurrentDictionary<string,ManagedGame> RunningGamesByGameId{ get; } = [];
    
    //Scheduled games
    private static ConcurrentDictionary<string,ScheduledGame> ScheduledGamesByGameId{ get; } = [];

    //Server status
    public static GameInfo[] RunningGames { get; set; } = [];
    public static ScheduledGameInfo[] ScheduledGames { get; set; } = [];
    public static LoggedInUserInfo[] RecentlySeenUsers { get; set; } = [];
    
    //Other
    private static DateTimeOffset LastRestored { get; set; }
    private static DateTimeOffset LastPersisted { get; set; }
    private static DateTimeOffset MaintenanceDate { get; set; }
    private static DateTimeOffset LastCleanedUpUserTokens { get; set; }
    private static DateTimeOffset LastCleanedUpScheduledGames { get; set; }
    
    private static DateTimeOffset LastUpdatedServerStatus { get; set; }
    
    private IConfiguration Configuration { get; } = configuration;

    private DbContextOptions<TreacheryContext> DbContextOptions { get; } = dbContextOptions;

    private TreacheryContext GetDbContext() => new(DbContextOptions, Configuration);

    private static string GenerateToken() => Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..16];
    
    private static VoidResult Error(ErrorType error, string errorDetails = "") 
        => new() { Error = error, ErrorDetails = errorDetails, Success = false }; 
    
    private static Result<TResult> Error<TResult>(ErrorType error, string errorDetails = "") 
        => new() { Error = error, ErrorDetails = errorDetails, Success = false }; 
    
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

        game.LastActivity = DateTimeOffset.Now;
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
        
        game.LastActivity = DateTimeOffset.Now;
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