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
    
    private async Task Cleanup()
    {
        await CleanupUserTokens();
        CleanupScheduledGames();
        
        _ = Task.Delay(CleanupTimeout).ContinueWith(_ => Cleanup());
    }
    
    private async Task CleanupUserTokens()
    {
        var now = DateTimeOffset.Now;
        if (!(now.Subtract(LastCleanup).TotalMinutes >= 15)) 
            return;

        LastCleanup = now;
        
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

    private void CleanupScheduledGames()
    {
        var thresholdDateTime = DateTimeOffset.Now.AddHours(-4);
        foreach (var gameIdAndGame in ScheduledGamesByGameId.Where(g => g.Value.DateTime < thresholdDateTime).ToArray())
        {
            ScheduledGamesByGameId.Remove(gameIdAndGame.Key, out _);
        }
    }

    public async Task<Result<string>> AdminUpdateMaintenance(string userToken, DateTimeOffset maintenanceDate)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Name != Configuration["GameAdminUsername"])
            return Error<string>(ErrorType.InvalidUserNameOrPassword);

        MaintenanceDate = maintenanceDate;
        return await Task.FromResult(Success("Maintenance window updated"));
    }

    public async Task<Result<string>> AdminPersistState(string userToken)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Name != Configuration["GameAdminUsername"])
            return Error<string>(ErrorType.InvalidUserNameOrPassword);
        
        var amountOfGames = 0;
        var amountOfScheduledGames = 0;
        await using (var context = GetDbContext())
        {
            await context.PersistedGames.ExecuteDeleteAsync();
            
            foreach (var gameIdAndManagedGame in RunningGamesByGameId)
            {
                var game = gameIdAndManagedGame.Value;

                var persisted = new PersistedGame
                {
                    CreationDate = game.CreationDate,
                    CreatorUserId = game.CreatorUserId,
                    GameId = gameIdAndManagedGame.Key,
                    GameState = GameState.GetStateAsString(game.Game),
                    GameName = game.Name,
                    GameParticipation = JsonSerializer.Serialize(game.Game.Participation),
                    HashedPassword = game.HashedPassword,
                    BotsArePaused = game.BotsArePaused,
                    ObserversRequirePassword = game.ObserversRequirePassword,
                    StatisticsSent = game.StatisticsSent,
                    LastAsyncPlayMessageSent = game.LastAsyncPlayMessageSent,
                };

                context.PersistedGames.Add(persisted);
                await context.SaveChangesAsync();
                amountOfGames++;
            }
            
            await context.ScheduledGames.ExecuteDeleteAsync();
            
            foreach (var gameIdAndManagedGame in ScheduledGamesByGameId)
            {
                var game = gameIdAndManagedGame.Value;

                var persisted = new PersistedScheduledGame
                {
                    DateTime = game.DateTime,
                    CreatorUserId = game.CreatorUserId,
                    GameId = gameIdAndManagedGame.Key,
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
            
        return Success($"Persisted: {amountOfGames} running games, {amountOfScheduledGames} scheduled games");
    }

    public async Task<Result<string>> AdminRestoreState(string userToken)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Name != Configuration["GameAdminUsername"])
            return Error<string>(ErrorType.InvalidUserNameOrPassword);
        
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
                var participation = JsonSerializer.Deserialize<GameParticipation>(persistedGame.GameParticipation);
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
                        BotsArePaused = persistedGame.BotsArePaused,
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
                    DateTime = scheduledGame.DateTime,
                    CreatorUserId = scheduledGame.CreatorUserId,
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
            
        return Success($"Restored: {amountRunning} running games, {amountScheduled} scheduled games");
    }

    public async Task<Result<string>> AdminCloseGame(string userToken, string gameId)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Name != Configuration["GameAdminUsername"])
            return Error<string>(ErrorType.InvalidUserNameOrPassword);

        if (RunningGamesByGameId.TryGetValue(gameId, out var game))
        {
            RunningGamesByGameId.Remove(gameId, out _);

            foreach (var userId in game.Game.Participation.Users.Keys)
            {
                game.Game.RemoveUser(userId, true);
                await Clients.Group(gameId).HandleRemoveUser(userId, true);
                await RemoveFromGroup(gameId, userId);
            }
        }
        
        return Success("Game removed");
    }
    
    public async Task<Result<string>> AdminDeleteUser(string userToken, int userId)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Name != Configuration["GameAdminUsername"])
            return Error<string>(ErrorType.InvalidUserNameOrPassword);

        await using var db = GetDbContext();
        await db.Users.Where(u => u.Id == userId).ExecuteDeleteAsync();
        
        return Success("User deleted");
    }
    
    public async Task<Result<AdminInfo>> GetAdminInfo(string userToken)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Name != Configuration["GameAdminUsername"])
            return Error<AdminInfo>(ErrorType.InvalidUserNameOrPassword);

        var result = new AdminInfo
        {
            Users = GetDbContext().Users.Select(u => new UserInfo { Id = u.Id, Name = u.Name, PlayerName = u.PlayerName, Email = u.Email, LastLogin = u.LastLogin }).ToList(),
            UsersByUserTokenCount = UsersByUserToken.Count,
            ConnectionInfoByUserIdCount = ConnectionInfoByUserId.Count,
            GamesByGameIdCount = RunningGamesByGameId.Count,
        };

        await Task.CompletedTask;
        return Success(result);
    }
}