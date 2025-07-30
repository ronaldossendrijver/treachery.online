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
        if (LastCleanedUpUserTokens.AddHours(CleanupFrequencyHours) > DateTimeOffset.Now)
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
        if (LastCleanedUpUserTokens.AddHours(CleanupFrequencyHours) > now)
            return;
        
        LastCleanedUpUserTokens = now;
        
        Log($"{nameof(CleanupUserTokensIfNeeded)} {UsersByUserToken.Count} {ConnectionInfoByUserId.Count}");
        
        foreach (var tokenAndInfo in UsersByUserToken.ToArray())
        {
            var age = now.Subtract(tokenAndInfo.Value.LoggedInDateTime).TotalDays;
            if (age >= MaximumLoginTimeDays)
            {
                UsersByUserToken.Remove(tokenAndInfo.Key, out _);
            } 
        }

        foreach (var userIdAndConnectionInfo in ConnectionInfoByUserId)
        {
            foreach (var gameId in userIdAndConnectionInfo.Value.GetGameIdsWithOldConnections(MaximumLoginTimeDays).ToArray())
            {
                await RemoveFromGroup(gameId, userIdAndConnectionInfo.Key);
            }
        }
    }

    private static void CleanupScheduledGamesIfNeeded()
    {
        if (LastCleanedUpScheduledGames.AddHours(CleanupFrequencyHours) > DateTimeOffset.Now)
            return;
        
        Log($"{nameof(CleanupScheduledGamesIfNeeded)} {ScheduledGamesByGameId.Count}");
        
        var thresholdDateTime = DateTimeOffset.Now.AddHours(-4);
        foreach (var gameIdAndGame in ScheduledGamesByGameId.Where(g => g.Value.DateTime < thresholdDateTime).ToArray())
        {
            ScheduledGamesByGameId.Remove(gameIdAndGame.Key, out _);
        }
        
        LastCleanedUpScheduledGames = DateTimeOffset.Now;
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
        
        var (amountOfNewGames, amountOfUpdatedGames, amountOfUnchanged, amountOfDeletedGames) = await PersistRunningGames();
        var amountOfScheduledGames = await PersistScheduledGames();
            
        return Success($"Games: {amountOfNewGames} new, {amountOfUpdatedGames} updated, {amountOfUnchanged} unchanged, {amountOfDeletedGames} deleted. Scheduled games: {amountOfScheduledGames}.");
    }
    
    private async Task PersistScheduledGamesIfNeeded()
    {
        if (LastPersistedScheduledGames.AddMinutes(PersistFrequencyMinutes) > DateTimeOffset.Now)
            return;

        await PersistScheduledGames();
    } 
    
    private async Task<(int amountOfNewGames, int amountOfUpdatedGames, int amountOfUnchanged, int amountOfDeletedGames)> PersistRunningGames()
    {
        if (LastRestored == default)
            return (0, 0, 0, 0);
        
        Log($"{nameof(PersistRunningGames)} started...");

        var amountOfNewGames = 0;
        var amountOfUpdatedGames = 0;
        var amountOfDeletedGames = 0;
        var amountOfUnchanged = 0;
        
        await using (var context = GetDbContext())
        {
            foreach (var (key, game) in RunningGamesByGameId)
            {
                var persistedGame = await context.PersistedGames.FirstOrDefaultAsync(g => g.GameId == game.GameId);
                if (persistedGame != null)
                {
                    if (persistedGame.LastAction == game.LastActivity)
                    {
                        amountOfUnchanged++;
                        continue;
                    }
                    
                    persistedGame.GameState = GameState.GetStateAsString(game.Game);
                    persistedGame.GameParticipation = Utilities.Serialize(game.Game.Participation);
                    persistedGame.StatisticsSent = game.StatisticsSent;
                    persistedGame.LastAsyncPlayMessageSent = game.LastAsyncPlayMessageSent;
                    persistedGame.LastAction = game.LastActivity;
                    context.PersistedGames.Update(persistedGame);
                    amountOfUpdatedGames++;
                }
                else
                {
                    context.PersistedGames.Add(new PersistedGame
                    {
                        CreationDate = game.CreationDate,
                        CreatorUserId = game.CreatorUserId,
                        GameId = key,
                        GameState = GameState.GetStateAsString(game.Game),
                        GameName = game.Name,
                        GameParticipation = Utilities.Serialize(game.Game.Participation),
                        HashedPassword = game.HashedPassword,
                        ObserversRequirePassword = game.ObserversRequirePassword,
                        StatisticsSent = game.StatisticsSent,
                        LastAsyncPlayMessageSent = game.LastAsyncPlayMessageSent,
                        LastAction = game.Game.History.LastOrDefault()?.Time ?? game.CreationDate
                    });
                    amountOfNewGames++;
                }

                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();
            }

            foreach (var persistedGame in (await context.PersistedGames.ToListAsync())
                     .Where(persistedGame => !RunningGamesByGameId.ContainsKey(persistedGame.GameId)))
            {
                context.Remove(persistedGame);
                amountOfDeletedGames++;
            }
                
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }
        
        Log($"{nameof(PersistRunningGames)}: {amountOfNewGames} new, {amountOfUpdatedGames} updated, {amountOfUnchanged} unchanged, {amountOfDeletedGames} deleted.");
        
        return (amountOfNewGames, amountOfUpdatedGames, amountOfUnchanged, amountOfDeletedGames);
    }
    
    private async Task<int> PersistScheduledGames()
    {
        if (LastRestored == default)
            return 0;

        LastPersistedScheduledGames = DateTimeOffset.Now;
        
        Log($"{nameof(PersistScheduledGames)} started...");
        
        var amountOfScheduledGames = 0;
        
        await using (var context = GetDbContext())
        {
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
                    SubscribedUsers = Utilities.Serialize(game.SubscribedUsers),
                    AsyncPlay = game.AsyncPlay
                };

                context.ScheduledGames.Add(persisted);
                await context.SaveChangesAsync();
                amountOfScheduledGames++;
            }
        }
        
        Log($"{nameof(PersistScheduledGames)}: {amountOfScheduledGames}.");
        
        return amountOfScheduledGames;
    }
    
    private async Task EraseGame(ManagedGame game)
    {
        await using var context = GetDbContext();
        var persistedGame = await context.PersistedGames.FirstOrDefaultAsync(g => g.GameId == game.GameId);
        if (persistedGame != null)
            context.Remove(persistedGame);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
    }

    private async Task PersistGameIfNeeded(ManagedGame game)
    {
        if (game.LastPersisted.AddMinutes(GamePersistFrequencyMinutes) > DateTimeOffset.Now)
            return;
        
        await PersistGame(game);
    }

    private async Task PersistGame(ManagedGame game)
    {
        game.LastPersisted = DateTimeOffset.Now;
        
        await using var context = GetDbContext();
        
        var persistedGame = await context.PersistedGames.FirstOrDefaultAsync(g => g.GameId == game.GameId);
        if (persistedGame != null)
        {
            if (persistedGame.LastAction == game.LastActivity)
                return;

            persistedGame.GameState = GameState.GetStateAsString(game.Game);
            persistedGame.GameParticipation = Utilities.Serialize(game.Game.Participation);
            persistedGame.StatisticsSent = game.StatisticsSent;
            persistedGame.LastAsyncPlayMessageSent = game.LastAsyncPlayMessageSent;
            persistedGame.LastAction = game.LastActivity;
            context.PersistedGames.Update(persistedGame);
        }
        else
        {
            context.PersistedGames.Add(new PersistedGame
            {
                CreationDate = game.CreationDate,
                CreatorUserId = game.CreatorUserId,
                GameId = game.GameId,
                GameState = GameState.GetStateAsString(game.Game),
                GameName = game.Name,
                GameParticipation = Utilities.Serialize(game.Game.Participation),
                HashedPassword = game.HashedPassword,
                ObserversRequirePassword = game.ObserversRequirePassword,
                StatisticsSent = game.StatisticsSent,
                LastAsyncPlayMessageSent = game.LastAsyncPlayMessageSent,
                LastAction = game.Game.History.LastOrDefault()?.Time ?? game.CreationDate
            });
        }

        await context.SaveChangesAsync();
    }

    public async Task<Result<string>> AdminRestoreState(string userToken)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Username != Configuration["GameAdminUsername"])
            return Error<string>(ErrorType.InvalidUserNameOrPassword);
        
        var (amountRunning, amountScheduled) = await RestoreGamesAndUserCache();

        return Success($"Restored: {amountRunning} running games, {amountScheduled} scheduled games");
    }

    private static bool Restoring { get; set; }
    
    private async Task RestoreGamesIfServerJustStarted()
    {
        if (LastRestored == default && !Restoring)
        {
            await RestoreGamesAndUserCache();
        }
    } 
    
    private async Task<(int amountRunning, int amountScheduled)> RestoreGamesAndUserCache()
    {
        Restoring = true;
        
        Log($"{nameof(RestoreGamesAndUserCache)} started...");
        
        var amountRunning = 0;
        var amountScheduled = 0;
        
        await using (var context = GetDbContext())
        {
            UsersById.Clear();
            foreach (var user in context.Users.AsNoTracking())
            {
                UsersById.TryAdd(user.Id, user);
            }

            RunningGamesByGameId.Clear();
            
            foreach (var persistedGame in context.PersistedGames.AsNoTracking())
            {
                var id = persistedGame.GameId;
                var gameState = GameState.Load(persistedGame.GameState);
                var gameName = persistedGame.GameName;
                var participation = Utilities.Deserialize<Participation>(persistedGame.GameParticipation);
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
                        LastActivity = persistedGame.LastAction,
                        LastAsyncPlayMessageSent = persistedGame.LastAsyncPlayMessageSent,
                    };
                    
                    RunningGamesByGameId.TryAdd(id, managedGame);
                    amountRunning++;
                }
            }
            
            ScheduledGamesByGameId.Clear();
            
            foreach (var scheduledGame in context.ScheduledGames.AsNoTracking())
            {
                var id = scheduledGame.GameId;
                var subscriptions = Utilities.Deserialize<Dictionary<int,SubscriptionType>>(scheduledGame.SubscribedUsers);
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
        
        LastRestored = DateTimeOffset.Now;
        
        Log($"{nameof(RestoreGamesAndUserCache)} {amountRunning} {amountScheduled}");

        Restoring = false;
        
        return (amountRunning, amountScheduled);
    }
    
    // private async Task RestoreGame(string gameId)
    // {
    //     await using var context = GetDbContext();
    //     var persistedGame = await context.PersistedGames.FirstOrDefaultAsync(g => g.GameId == gameId);
    //     if (persistedGame == null)
    //         return;
    //     
    //     var id = persistedGame.GameId;
    //     var gameState = GameState.Load(persistedGame.GameState);
    //     var gameName = persistedGame.GameName;
    //     var participation = Utilities.Deserialize<Participation>(persistedGame.GameParticipation);
    //     var loadMessage = Game.TryLoad(gameState, participation, false, true, out var game);
    //     if (loadMessage == null)
    //     {
    //         var managedGame = new ManagedGame
    //         {
    //             CreationDate = persistedGame.CreationDate,
    //             CreatorUserId = persistedGame.CreatorUserId,
    //             GameId = persistedGame.GameId,
    //             Game = game,
    //             Name = gameName,
    //             HashedPassword = persistedGame.HashedPassword,
    //             ObserversRequirePassword = persistedGame.ObserversRequirePassword,
    //             StatisticsSent = persistedGame.StatisticsSent,
    //             LastActivity = persistedGame.LastAction,
    //             LastAsyncPlayMessageSent = persistedGame.LastAsyncPlayMessageSent,
    //         };
    //                 
    //         RunningGamesByGameId[id] = managedGame;
    //     }
    // }

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

        ScheduledGamesByGameId.Remove(scheduledGameId, out _);
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
    
    public async Task<VoidResult> RequestNudgeBots(string userToken, string gameId)
    {
        if (!AreValid(userToken, gameId, out _, out var game, out var error))
            return error;

        await ScheduleBotEvent(game, true);

        await Task.CompletedTask;
        return Success();
    }
}