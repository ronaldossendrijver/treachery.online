using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Treachery.Server;

public partial class GameHub
{
    public async Task<Result<ServerInfo>> Connect()
    {
        await Task.CompletedTask;
        
        var result = new ServerInfo
        {
            AdminName = Configuration["GameAdminUsername"],
            ScheduledMaintenance = MaintenanceDate,
        };

        return Success(result);
    }

    private async Task CleanupUserTokensIfNeeded()
    {
        if (LastCleanedUp.AddMilliseconds(CleanupFrequency) > DateTimeOffset.Now)
            return;

        await CleanupUserTokens();
    }

    private static void Log(string message)
    {
        Console.WriteLine(message);
    }

    private async Task CleanupUserTokens()
    {
        var now = DateTimeOffset.Now;
        if (LastCleanedUp.AddMilliseconds(CleanupFrequency) > now)
            return;
        
        LastCleanedUp = now;
        
        Log($"{nameof(CleanupUserTokensIfNeeded)} {UsersByUserToken.Count} {ConnectionInfoByUserId.Count}");
        
        foreach (var tokenAndInfo in UsersByUserToken.ToArray())
        {
            var age = now.Subtract(tokenAndInfo.Value.LoggedInDateTime).TotalMinutes;
            if (age >= 10080 ||
                age >= 15 && now.Subtract(tokenAndInfo.Value.LastSeenDateTime).TotalMinutes >= 15)
            {
                UsersByUserToken.Remove(tokenAndInfo.Key, out _);
            } 
        }

        foreach (var userIdAndConnectionInfo in ConnectionInfoByUserId)
        {
            foreach (var gameId in userIdAndConnectionInfo.Value.GetGameIdsWithOldConnections(10080).ToArray())
            {
                await RemoveFromGroup(gameId, userIdAndConnectionInfo.Key);
            }
        }
    }

    public static void RunAndRescheduleCleanupScheduledGames()
    {
        Log($"{nameof(RunAndRescheduleCleanupScheduledGames)} {ScheduledGamesByGameId.Count}");
        
        var thresholdDateTime = DateTimeOffset.Now.AddHours(-4);
        foreach (var gameIdAndGame in ScheduledGamesByGameId.Where(g => g.Value.DateTime < thresholdDateTime).ToArray())
        {
            ScheduledGamesByGameId.Remove(gameIdAndGame.Key, out _);
        }
        
        _ = Task.Delay(CleanupFrequency).ContinueWith(_ => RunAndRescheduleCleanupScheduledGames());
    }

    public async Task<Result<string>> AdminUpdateMaintenance(string userToken, DateTimeOffset maintenanceDate)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Username != Configuration["GameAdminUsername"])
            return Error<string>(ErrorType.InvalidUserNameOrPassword);

        MaintenanceDate = maintenanceDate;
        return await Task.FromResult(Success("Maintenance window updated"));
    }

    public async Task<Result<string>> AdminPersistState(string userToken)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Username != Configuration["GameAdminUsername"])
            return Error<string>(ErrorType.InvalidUserNameOrPassword);
        
        var (amountRunning, amountScheduled)  = await PersistGames();
            
        return Success($"Persisted: {amountRunning} running games, {amountScheduled} scheduled games");
    }

    private async Task RestoreGamesIfNeeded()
    {
        if (LastRestored == default)
        {
            await RestoreGames();
        }
    } 
    
    private async Task PersistGamesIfNeeded()
    {
        if (LastRestored == default || LastPersisted.AddMilliseconds(PersistFrequency) > DateTimeOffset.Now)
            return;

        await PersistGames();
    } 
    
    private async Task<(int amountRunning, int amountScheduled)> PersistGames()
    {
        LastPersisted = DateTimeOffset.Now;
        
        var amountOfGames = 0;
        var amountOfScheduledGames = 0;
        
        await using (var context = GetDbContext())
        {
            await context.PersistedGames.ExecuteDeleteAsync();
            
            foreach (var (key, game) in RunningGamesByGameId)
            {
                var persisted = new PersistedGame
                {
                    CreationDate = game.CreationDate,
                    CreatorUserId = game.CreatorUserId,
                    GameId = key,
                    GameState = GameState.GetStateAsString(game.Game),
                    GameName = game.Name,
                    GameParticipation = JsonSerializer.Serialize(game.Game.Participation),
                    HashedPassword = game.HashedPassword,
                    ObserversRequirePassword = game.ObserversRequirePassword,
                    StatisticsSent = game.StatisticsSent,
                    LastAsyncPlayMessageSent = game.LastAsyncPlayMessageSent,
                };

                context.PersistedGames.Add(persisted);
                await context.SaveChangesAsync();
                amountOfGames++;
            }
            
            await context.ScheduledGames.ExecuteDeleteAsync();
            
            foreach (var (key, game) in ScheduledGamesByGameId)
            {
                var persisted = new PersistedScheduledGame
                {
                    DateTime = game.DateTime,
                    CreatorUserId = game.CreatorUserId,
                    CreatorPlayerName = game.CreatorPlayerName,
                    GameId = key,
                    Ruleset = game.Ruleset,
                    MaximumTurns = game.MaximumTurns,
                    NumberOfPlayers = game.NumberOfPlayers,
                    AllowedFactionsInPlay = game.AllowedFactionsInPlay,
                    SubscribedUsers = JsonSerializer.Serialize(game.SubscribedUsers),
                    AsyncPlay = game.AsyncPlay
                };

                context.ScheduledGames.Add(persisted);
                await context.SaveChangesAsync();
                amountOfScheduledGames++;
            }
        }
        
        Log($"{nameof(PersistGames)} {amountOfGames} {amountOfScheduledGames}");
        
        return (amountOfGames, amountOfScheduledGames);
    }

    public async Task<Result<string>> AdminRestoreState(string userToken)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Username != Configuration["GameAdminUsername"])
            return Error<string>(ErrorType.InvalidUserNameOrPassword);
        
        var (amountRunning, amountScheduled) = await RestoreGames();

        return Success($"Restored: {amountRunning} running games, {amountScheduled} scheduled games");
    }

    private async Task<(int amountRunning, int amountScheduled)> RestoreGames()
    {
        var amountRunning = 0;
        var amountScheduled = 0;
        
        await using (var context = GetDbContext())
        {
            RunningGamesByGameId.Clear();
            
            foreach (var persistedGame in context.PersistedGames)
            {
                var id = persistedGame.GameId;
                var gameState = GameState.Load(persistedGame.GameState);
                var gameName = persistedGame.GameName;
                var participation = JsonSerializer.Deserialize<Participation>(persistedGame.GameParticipation);
                var loadMessage = Game.TryLoad(gameState, participation, false, true, out var game);
                if (loadMessage == null)
                {
                    var managedGame = new ManagedGame
                    {
                        CreationDate = persistedGame.CreationDate,
                        CreatorUserId = persistedGame.CreatorUserId,
                        GameId = persistedGame.GameId,
                        Game = game,
                        Name = gameName,
                        HashedPassword = persistedGame.HashedPassword,
                        ObserversRequirePassword = persistedGame.ObserversRequirePassword,
                        StatisticsSent = persistedGame.StatisticsSent,
                        LastAsyncPlayMessageSent = persistedGame.LastAsyncPlayMessageSent,
                    };
                    
                    RunningGamesByGameId.TryAdd(id, managedGame);
                    amountRunning++;
                }
            }
            
            ScheduledGamesByGameId.Clear();
            
            foreach (var scheduledGame in context.ScheduledGames)
            {
                var id = scheduledGame.GameId;
                var subscriptions = JsonSerializer.Deserialize<Dictionary<int,SubscriptionType>>(scheduledGame.SubscribedUsers);
                var game = new ScheduledGame
                {
                    ScheduledGameId = scheduledGame.GameId,
                    DateTime = scheduledGame.DateTime,
                    CreatorUserId = scheduledGame.CreatorUserId,
                    CreatorPlayerName = scheduledGame.CreatorPlayerName,
                    Ruleset = scheduledGame.Ruleset,
                    MaximumTurns = scheduledGame.MaximumTurns,
                    NumberOfPlayers = scheduledGame.NumberOfPlayers,
                    AllowedFactionsInPlay = scheduledGame.AllowedFactionsInPlay,
                    SubscribedUsers = subscriptions,
                    AsyncPlay = scheduledGame.AsyncPlay
                };
                
                ScheduledGamesByGameId.TryAdd(id, game);
                amountScheduled++;
            }
        }
        
        Log($"{nameof(RestoreGames)} {amountRunning} {amountScheduled}");
        
        LastRestored = DateTimeOffset.Now;
        LastPersisted = LastRestored;

        return (amountRunning, amountScheduled);
    }

    public async Task<Result<string>> AdminCloseGame(string userToken, string gameId)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Username != Configuration["GameAdminUsername"])
            return Error<string>(ErrorType.InvalidUserNameOrPassword);

        if (RunningGamesByGameId.TryGetValue(gameId, out var game))
        {
            RunningGamesByGameId.Remove(gameId, out _);

            foreach (var userId in game.Game.Participation.PlayerNames.Keys)
            {
                game.Game.RemoveUser(userId, true);
                await Clients.Group(gameId).HandleRemoveUser(userId, true);
                await RemoveFromGroup(gameId, userId);
            }
        }
        
        return Success("Game removed");
    }
    
    public async Task<Result<string>> AdminCancelGame(string userToken, string scheduledGameId)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Username != Configuration["GameAdminUsername"])
            return Error<string>(ErrorType.InvalidUserNameOrPassword);

        if (ScheduledGamesByGameId.TryGetValue(scheduledGameId, out var game))
        {
            ScheduledGamesByGameId.Remove(scheduledGameId, out _);
        }
        
        return await Task.FromResult(Success("Scheduled game cancelled"));
    }
    
    public async Task<Result<string>> AdminDeleteUser(string userToken, int userId)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Username != Configuration["GameAdminUsername"])
            return Error<string>(ErrorType.InvalidUserNameOrPassword);

        await using var db = GetDbContext();
        await db.Users.Where(u => u.Id == userId).ExecuteDeleteAsync();
        
        return Success("User deleted");
    }
    
    public async Task<Result<AdminInfo>> GetAdminInfo(string userToken)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Username != Configuration["GameAdminUsername"])
            return Error<AdminInfo>(ErrorType.InvalidUserNameOrPassword);

        var result = new AdminInfo
        {
            Users = GetDbContext().Users.Select(u => new UserInfo { Id = u.Id, Username = u.Name, PlayerName = u.PlayerName, Email = u.Email, LastLogin = u.LastLogin }).ToList(),
            UsersByUserTokenCount = UsersByUserToken.Count,
            ConnectionInfoByUserIdCount = ConnectionInfoByUserId.Count,
            GamesByGameIdCount = RunningGamesByGameId.Count,
        };

        await Task.CompletedTask;
        return Success(result);
    }
}