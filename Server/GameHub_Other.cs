namespace Treachery.Server;

public partial class GameHub
{
    private static ConcurrentDictionary<string, DateTimeOffset> LastClientErrorReportByConnectionId { get; } = [];

    public async Task<Result<ServerInfo>> Connect()
    {
        await Task.CompletedTask;
        
        var result = new ServerInfo
        {
            AdminName = Configuration["GameAdminUsername"] ?? "admin",
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
        
        await RestoreGamesIfServerJustStarted();
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
        
        var now = DateTimeOffset.Now;
        
        Log($"{nameof(PersistRunningGames)} started...");

        var amountOfNewGames = 0;
        var amountOfUpdatedGames = 0;
        var amountOfDeletedGames = 0;
        var amountOfUnchanged = 0;
        
        await using (var context = GetDbContext())
        {
            foreach (var (key, game) in RunningGamesByGameId.Where(x => x.Value.IsLoaded && x.Value.LastActivity > LastPersistedRunningGames))
            {
                var persistedGame = await context.PersistedGames.FirstOrDefaultAsync(g => g.GameId == game.GameId);
                if (persistedGame != null)
                {
                    if (persistedGame.LastAction == game.LastActivity && persistedGame.GameInfo != null)
                    {
                        amountOfUnchanged++;
                        continue;
                    }
                    
                    persistedGame.GameState = GameState.GetStateAsString(game.Game);
                    persistedGame.GameInfo = Utilities.Serialize(GameInfo.FromGame(game.Game));
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
                        GameInfo = Utilities.Serialize(GameInfo.FromGame(game.Game)),
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

            foreach (var persistedGame in await context.PersistedGames.AsNoTracking()
                         .Select(persistedGame => new { persistedGame.Id, persistedGame.GameId })
                         .ToListAsync())
            {
                if (!RunningGamesByGameId.ContainsKey(persistedGame.GameId))
                {
                    context.Remove(new PersistedGame { Id = persistedGame.Id });
                    GameInfoSummaryCache.Remove(persistedGame.GameId);
                    amountOfDeletedGames++;
                }
            }

            LastPersistedRunningGames = now;
                
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
        GameInfoSummaryCache.Remove(game.GameId);
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
            if (persistedGame.LastAction == game.LastActivity && persistedGame.GameInfo != null)
                return;

            persistedGame.GameState = GameState.GetStateAsString(game.Game);
            persistedGame.GameInfo = Utilities.Serialize(GameInfo.FromGame(game.Game));
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
                GameInfo = Utilities.Serialize(GameInfo.FromGame(game.Game)),
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

    // Games created before passwords were hashed on the client stored the plain password.
    private static string NormalizeLegacyGamePassword(string storedPassword)
    {
        if (string.IsNullOrEmpty(storedPassword) || ValidatePasswordHash(storedPassword) == ErrorType.None)
            return storedPassword;

        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(storedPassword));
        return Convert.ToHexStringLower(bytes);
    }
    
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

            var recentGameThreshold = DateTimeOffset.Now.AddDays(-RecentGameLoadDays);
            var persistedGames = await context.PersistedGames.AsNoTracking()
                .Select(persistedGame => new
                {
                    persistedGame.Id,
                    persistedGame.GameId,
                    persistedGame.GameName,
                    persistedGame.CreationDate,
                    persistedGame.CreatorUserId,
                    persistedGame.GameParticipation,
                    persistedGame.GameInfo,
                    persistedGame.HashedPassword,
                    persistedGame.ObserversRequirePassword,
                    persistedGame.StatisticsSent,
                    persistedGame.LastAsyncPlayMessageSent,
                    persistedGame.LastAction
                })
                .ToListAsync();

            foreach (var persistedGame in persistedGames)
            {
                var id = persistedGame.GameId;

                try
                {
                    var participation = Utilities.Deserialize<Participation>(persistedGame.GameParticipation) ?? new Participation();
                    var managedGame = new ManagedGame
                    {
                        CreationDate = persistedGame.CreationDate,
                        CreatorUserId = persistedGame.CreatorUserId,
                        GameId = persistedGame.GameId,
                        Name = persistedGame.GameName,
                        HashedPassword = NormalizeLegacyGamePassword(persistedGame.HashedPassword),
                        ObserversRequirePassword = persistedGame.ObserversRequirePassword,
                        StatisticsSent = persistedGame.StatisticsSent,
                        LastActivity = persistedGame.LastAction,
                        LastAsyncPlayMessageSent = persistedGame.LastAsyncPlayMessageSent,
                        Participation = participation
                    };

                    if (!string.IsNullOrEmpty(persistedGame.GameInfo))
                    {
                        var summary = Utilities.Deserialize<GameInfo>(persistedGame.GameInfo);
                        if (summary?.HasDetails == true)
                            GameInfoSummaryCache.Set(id, summary);
                    }

                    if (persistedGame.LastAction >= recentGameThreshold)
                    {
                        var gameStateData = await context.PersistedGames.AsNoTracking()
                            .Where(game => game.Id == persistedGame.Id)
                            .Select(game => game.GameState)
                            .SingleAsync();
                        var loadMessage = Game.TryLoad(GameState.Load(gameStateData), participation, false, true, out var loadedGame);
                        if (loadMessage != null || loadedGame is null)
                        {
                            Log($"Unable to restore game {id}: {loadMessage?.ToString() ?? "Unknown error"}");
                            continue;
                        }

                        managedGame.Game = loadedGame;
                        managedGame.Participation = loadedGame.Participation;
                    }

                    RunningGamesByGameId.TryAdd(id, managedGame);
                    amountRunning++;
                }
                catch (Exception ex)
                {
                    Log("Unable to restore game " + id + ": " + ex);
                }
            }

            ScheduledGamesByGameId.Clear();
            
            foreach (var scheduledGame in context.ScheduledGames.AsNoTracking())
            {
                var id = scheduledGame.GameId;
                
                var subscriptions = Utilities.Deserialize<Dictionary<int,SubscriptionType>>(scheduledGame.SubscribedUsers) ?? [];
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
        LastPersistedRunningGames = LastRestored;
        
        Log($"{nameof(RestoreGamesAndUserCache)} {amountRunning} {amountScheduled}");

        Restoring = false;
        
        return (amountRunning, amountScheduled);
    }

    private async Task<VoidResult> EnsureGameLoaded(ManagedGame game)
    {
        var errorDetails = await game.LoadIfNeededAsync(async () =>
        {
            try
            {
                await using var context = GetDbContext();
                var gameStateData = await context.PersistedGames.AsNoTracking()
                    .Where(persistedGame => persistedGame.GameId == game.GameId)
                    .Select(persistedGame => persistedGame.GameState)
                    .SingleOrDefaultAsync();
                if (gameStateData is null)
                    return (null, "The saved game could not be found in storage.");

                var loadMessage = Game.TryLoad(GameState.Load(gameStateData), game.Participation, false, true, out var loadedGame);
                return loadMessage is null && loadedGame is not null
                    ? (loadedGame, null)
                    : (null, loadMessage?.ToString() ?? "The saved game could not be restored.");
            }
            catch (Exception ex)
            {
                Log($"Unable to load saved game {game.GameId}: {ex}");
                return (null, ex.Message);
            }
        });

        return errorDetails is null
            ? Success()
            : Error(ErrorType.InvalidGameEvent, errorDetails);
    }
    
    public async Task<Result<string>> AdminCloseGame(string userToken, string gameId)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Username != Configuration["GameAdminUsername"])
            return Error<string>(ErrorType.InvalidUserNameOrPassword);

        if (RunningGamesByGameId.TryGetValue(gameId, out var game))
        {
            RunningGamesByGameId.Remove(gameId, out _);

            foreach (var userId in game.Participation.PlayerNames.Keys)
            {
                await Clients.Group(gameId).HandleRemoveUser(userId, true);
                await RemoveFromGroup(gameId, userId);
            }
        }
        
        return Success("Game removed");
    }

    public async Task<Result<string>> AdminDownloadGame(string userToken, string gameId)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Username != Configuration["GameAdminUsername"])
            return Error<string>(ErrorType.InvalidUserNameOrPassword);

        if (!RunningGamesByGameId.TryGetValue(gameId, out var game))
            return Error<string>(ErrorType.GameNotFound);

        var loadResult = await EnsureGameLoaded(game);
        if (!loadResult.Success)
            return Error<string>(loadResult.Error, loadResult.ErrorDetails);

        await Task.CompletedTask;
        return Success(GameState.GetStateAsString(game.Game));
    }
    
    public async Task<Result<string>> AdminCancelGame(string userToken, string scheduledGameId)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Username != Configuration["GameAdminUsername"])
            return Error<string>(ErrorType.InvalidUserNameOrPassword);

        ScheduledGamesByGameId.Remove(scheduledGameId, out _);
        return await Task.FromResult(Success("Scheduled game cancelled"));
    }
    
    public async Task<Result<string>> DeleteUser(string userToken, int userId)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user))
            return Error<string>(ErrorType.InvalidUserNameOrPassword);

        if (userId != user.Id && user.Username != Configuration["GameAdminUsername"])
            return Error<string>(ErrorType.NotAdmin);

        foreach (var game in RunningGamesByGameId.Values.Where(x => x.CreatorUserId == userId).ToArray())
        {
            foreach (var participatingUserId in game.Participation.PlayerNames.Keys)
            {
                await Clients.Group(game.GameId).HandleRemoveUser(participatingUserId, true);
                await RemoveFromGroup(game.GameId, participatingUserId);
            }
            
            RunningGamesByGameId.Remove(game.GameId, out _);
            await EraseGame(game);
        }

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

    public async Task<Result<ErrorLogInfo[]>> GetAdminErrorLog(
        string userToken,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int? gameVersion)
    {
        if (string.IsNullOrEmpty(userToken) || !UsersByUserToken.TryGetValue(userToken, out var user) ||
            user.Username != Configuration["GameAdminUsername"])
            return Error<ErrorLogInfo[]>(ErrorType.InvalidUserNameOrPassword);

        await using var context = GetDbContext();
        var query = context.ErrorLogs.AsNoTracking();
        if (from.HasValue)
            query = query.Where(entry => entry.OccurredAt >= from.Value.UtcDateTime);
        if (to.HasValue)
            query = query.Where(entry => entry.OccurredAt <= to.Value.UtcDateTime);
        if (gameVersion.HasValue)
            query = query.Where(entry => entry.GameVersion == gameVersion.Value);

        var logs = await query
            .OrderByDescending(entry => entry.OccurredAt)
            .Take(500)
            .Select(entry => new ErrorLogInfo
            {
                Id = entry.Id,
                OccurredAt = new DateTimeOffset(DateTime.SpecifyKind(entry.OccurredAt, DateTimeKind.Utc)),
                GameVersion = entry.GameVersion,
                Source = entry.Source,
                Message = entry.Message,
                Details = entry.Details,
                Url = entry.Url,
                UserAgent = entry.UserAgent,
                UserId = entry.UserId,
                Username = entry.Username
            })
            .ToArrayAsync();

        return Success(logs);
    }

    public async Task<VoidResult> ReportClientError(string? userToken, ClientErrorReport report)
    {
        if (report is null || report.Source is not ("JavaScript" or "Blazor"))
            return Error(ErrorType.InvalidGameEvent);

        var now = DateTimeOffset.UtcNow;
        var connectionId = Context.ConnectionId;
        while (true)
        {
            var lastReport = LastClientErrorReportByConnectionId.GetOrAdd(connectionId, DateTimeOffset.MinValue);
            if (now - lastReport < TimeSpan.FromSeconds(2))
                return Error(ErrorType.InvalidGameEvent, "Client error reporting rate limited.");
            if (LastClientErrorReportByConnectionId.TryUpdate(connectionId, now, lastReport))
                break;
        }

        GameHub.TryGetLoggedInUser(userToken, out var user);
        var details = report.Details;
        if (report.LineNumber.HasValue)
            details += $"\nLine: {report.LineNumber}, column: {report.ColumnNumber}";

        var httpContext = Context.GetHttpContext()
                         ?? throw new InvalidOperationException("Client error reports require an HTTP connection.");
        var errorLog = httpContext.RequestServices.GetRequiredService<ErrorLogService>();
        await errorLog.RecordAsync(
            $"Client/{report.Source}",
            report.Message,
            details,
            report.Url,
            report.UserAgent,
            user?.Id,
            user?.Username);

        return Success();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        LastClientErrorReportByConnectionId.TryRemove(Context.ConnectionId, out _);
        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task<VoidResult> RequestNudgeBots(string userToken, string gameId)
    {
        if (!AreValid(userToken, gameId, out _, out var game, out var error))
            return error!;

        ScheduleBotEvent(game!, true);

        await Task.CompletedTask;
        return Success();
    }
}