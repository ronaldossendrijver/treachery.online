using System.Net.Http;
using System.Text;

namespace Treachery.Server;

public partial class GameHub
{
    public async Task<Result<GameInitInfo>> RequestCreateGame(string userToken, string hashedPassword, string stateData, string skin)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user))
            return Error<GameInitInfo>(ErrorType.UserNotFound);
        
        if (RunningGamesByGameId.Values.Count(g => g.CreatorUserId == user.Id) >= 3)
            return Error<GameInitInfo>(ErrorType.TooManyGames);

        Game game;
        var loadGame = !string.IsNullOrEmpty(stateData);
        if (loadGame)
        {
            var state = GameState.Load(stateData);
            var errorMessage = Game.TryLoad(state, new Participation(), false, false, out var loadedGame);

            if (errorMessage == null)
                game = loadedGame;
            else
                return Error<GameInitInfo>(ErrorType.InvalidGameEvent, errorMessage.ToString());
        }
        else
        {
            game = new Game();
        }

        var gameId = Guid.NewGuid().ToString();
        var managedGame = new ManagedGame
        {
            CreationDate = DateTimeOffset.Now,
            LastActivity = DateTimeOffset.Now,
            CreatorUserId = user.Id,
            GameId = gameId,
            Game = game,
            Name = user.PlayerName,
            HashedPassword = hashedPassword,
            ObserversRequirePassword = false
        };
        
        RunningGamesByGameId[gameId] = managedGame;

        game.AddPlayer(user.Id, user.PlayerName);
        game.SetOrUnsetHost(user.Id);
        await AddToGroup(gameId, user.Id, Context.ConnectionId);
        
        if (!string.IsNullOrEmpty(skin))
            await Clients.Group(gameId).HandleSetSkin(skin);
        
        await PersistGameIfNeeded(managedGame);
        
        return Success(new GameInitInfo
        {
            GameId = gameId, 
            GameState = stateData ?? GameState.GetStateAsString(game), 
            GameName = managedGame.Name,
            Participation = game.Participation
        });
    }

    public async Task<Result<ServerStatus>> RequestScheduleGame(string userToken, DateTimeOffset dateTime, Ruleset? ruleset,
        int? numberOfPlayers, int? maximumTurns, List<Faction> allowedFactionsInPlay, bool asyncPlay)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user))
            return Error<ServerStatus>(ErrorType.UserNotFound);
        
        if (ScheduledGamesByGameId.Values.Count(g => g.CreatorUserId == user.Id) >= 3)
            return Error<ServerStatus>(ErrorType.TooManyScheduledGames);

        var gameId = Guid.NewGuid().ToString();
        var scheduledGame = new ScheduledGame
        {
            ScheduledGameId = gameId,
            DateTime = dateTime,
            CreatorUserId = user.Id,
            CreatorPlayerName = user.PlayerName,
            Ruleset = ruleset,
            NumberOfPlayers = numberOfPlayers,
            MaximumTurns = maximumTurns,
            AllowedFactionsInPlay = allowedFactionsInPlay,
            AsyncPlay = asyncPlay,
            SubscribedUsers = { [user.Id] = SubscriptionType.CertainAsHost }
        };

        ScheduledGamesByGameId[gameId] = scheduledGame;
        
        UpdateServerStatusIfNeeded(true);
        await Task.CompletedTask;
        
        return Success(FilteredServerStatus(GameListScope.Active, user.Id));
    }
    
    public async Task<Result<ServerStatus>> RequestCancelGame(string userToken, string scheduledGameId)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user))
            return Error<ServerStatus>(ErrorType.UserNotFound);
        
        if (!ScheduledGamesByGameId.TryGetValue(scheduledGameId, out var scheduledGame)) 
            return Error<ServerStatus>(ErrorType.ScheduledGameNotFound);
        
        if (user.Id != scheduledGame.CreatorUserId)
            return Error<ServerStatus>(ErrorType.NoHost);

        ScheduledGamesByGameId.Remove(scheduledGameId, out _);
        
        UpdateServerStatusIfNeeded(true);
        await Task.CompletedTask;
        
        return Success(FilteredServerStatus(GameListScope.Active, user.Id));
    }
    
    public async Task<VoidResult> RequestLoadGame(string userToken, string gameId, string stateData, string skin)
    {
        if (!AreValid(userToken, gameId, out var user, out var game, out var error))
            return error;
        
        if (!game.Game.IsHost(user.Id))
            return Error(ErrorType.NoHost);
        
        var state = GameState.Load(stateData);
        var errorMessage = Game.TryLoad(state, game.Game.Participation, false, false, out var loadedGame);

        if (errorMessage != null)
            return Error(ErrorType.InvalidGameEvent, errorMessage.ToString());

        if (loadedGame.Seed != game.Game.Seed)
            loadedGame.ResetSeats();
        
        game.Game = loadedGame;
        await Clients.Group(gameId).HandleLoadGame(new GameInitInfo
        {
            GameId = gameId, 
            GameState = stateData, 
            GameName = game.Name,
            Participation = game.Game.Participation
        });

        if (!string.IsNullOrEmpty(skin))
            await Clients.Group(gameId).HandleSetSkin(skin);
        
        await PersistGameIfNeeded(game);

        await NudgeBots(game);
        return Success();
    }

    private async Task NudgeBots(ManagedGame game)
    {
        if (!game.Game.Participation.BotsArePaused)
        {
            await PerformBotEvent(game);
        }
    }

    public async Task<VoidResult> RequestAssignSeats(string userToken, string gameId, Dictionary<int, int> assignment)
    {
        if (!AreValid(userToken, gameId, out var user, out var game, out var error))
            return error;

        if (!game.Game.IsHost(user.Id))
            return Error(ErrorType.NoHost);

        game.Game.Participation.SeatedPlayers = assignment;
        await Clients.Group(gameId).HandleAssignSeats(assignment);
        
        await PersistGameIfNeeded(game);

        await NudgeBots(game);
        return Success();
    }

    public async Task<Result<GameInitInfo>> RequestJoinGame(string userToken, string gameId, string hashedPassword, int seat)
    {
        if (!AreValid<GameInitInfo>(userToken, gameId, out var user, out var game, out var error))
            return error;
        
        if (!game.Game.IsPlayer(user.Id))
        {
            if (!string.IsNullOrEmpty(game.HashedPassword) && !game.HashedPassword.Equals(hashedPassword))
                return Error<GameInitInfo>(ErrorType.IncorrectGamePassword);
        
            if (!game.Game.IsOpen(seat))
                return Error<GameInitInfo>(ErrorType.SeatNotAvailableNotOpen);
            
            if (game.Game.WasKicked(user.Id))
                return Error<GameInitInfo>(ErrorType.SeatNotAvailableKicked);
            
            if (seat >= 0)
            {
                var currentUserId = game.Game.UserIdInSeat(seat);
                if (currentUserId >= 0)
                {
                    game.Game.RemoveUser(currentUserId, false);
                    await Clients.Group(gameId).HandleRemoveUser(currentUserId, false);
                }
            }
            
            game.Game.AddPlayer(user.Id, user.PlayerName, seat);
            await Clients.Group(gameId).HandleJoinGame(user.Id, user.PlayerName, seat);
            
            if (game.Game.NumberOfPlayers == 1 || game.CreatorUserId == user.Id)
            {
                game.Game.SetOrUnsetHost(user.Id);
                await Clients.Group(gameId).HandleSetOrUnsetHost(user.Id);
            }
        }
      
        await AddToGroup(gameId, user.Id, Context.ConnectionId);
        
        await PersistGameIfNeeded(game);
        
        if (game.Game.NumberOfPlayers == 1)
            await NudgeBots(game);
            
        return Success(new GameInitInfo
        {
            GameId = gameId, 
            GameState = GameState.GetStateAsString(game.Game), 
            GameName = game.Name,
            Participation = game.Game.Participation
        });
    }
    
    public async Task<Result<ServerStatus>> RequestSubscribeGame(string userToken, string gameId, SubscriptionType subscription)
    {
        if (gameId == null)
            return Error<ServerStatus>(ErrorType.GameNotFound);
        
        if (!UsersByUserToken.TryGetValue(userToken, out var user))
        {
            return Error<ServerStatus>(ErrorType.UserNotFound);
        }
        
        if (!ScheduledGamesByGameId.TryGetValue(gameId, out var game))
        {
            return Error<ServerStatus>(ErrorType.GameNotFound);
        }

        game.SubscribedUsers[user.Id] = subscription;

        UpdateServerStatusIfNeeded(true);

        await Task.CompletedTask;
        
        return Success(FilteredServerStatus(GameListScope.Active, user.Id));
    }

    public async Task<VoidResult> RequestOpenOrCloseSeat(string userToken, string gameId, int seat)
    {
        if (!AreValid(userToken, gameId, out var user, out var game, out var error))
            return error;
        
        if (!game.Game.IsHost(user.Id))
            return Error(ErrorType.NoHost);

        game.Game.OpenOrCloseSeat(seat);
        await Clients.Group(gameId).HandleOpenOrCloseSeat(seat);
        
        await PersistGameIfNeeded(game);
        
        return Success();
    }
    
    public async Task<VoidResult> RequestSetOrUnsetHost(string userToken, string gameId, int userId)
    {
        if (!AreValid(userToken, gameId, out var host, out var game, out var error))
            return error;

        if (!game.Game.IsHost(host.Id))
            return Error(ErrorType.NoHost);

        if (userId == host.Id && game.Game.NumberOfHosts <= 1) 
            return Error(ErrorType.CannotRemoveLastHost);
        
        game.Game.SetOrUnsetHost(userId);
        await Clients.Group(gameId).HandleSetOrUnsetHost(userId);
        
        await PersistGameIfNeeded(game);
        
        return Success();
    }

    public async Task<Result<ServerStatus>> RequestLeaveGame(string userToken, string gameId)
    {
        if (!AreValid<ServerStatus>(userToken, gameId, out var user, out var game, out var error))
            return error;
        
        if (game.Game.IsHost(user.Id) && game.Game.NumberOfHosts == 1)
            foreach (var id in game.Game.Participation.SeatedPlayers.Keys.Where(playerUserId =>
                         playerUserId != user.Id))
            {
                game.Game.SetOrUnsetHost(id);
                await Clients.Group(gameId).HandleSetOrUnsetHost(id);
            }
                
        game.Game.RemoveUser(user.Id, false);
        await Clients.Group(gameId).HandleRemoveUser(user.Id, false);
        await RemoveFromGroup(gameId, user.Id);
        
        await PersistGameIfNeeded(game);
        
        await NudgeBots(game);
        UpdateServerStatusIfNeeded(true);
        
        return Success(FilteredServerStatus(GameListScope.Active, user.Id));
    }

    public async Task<VoidResult> RequestKick(string userToken, string gameId, int userId)
    {
        if (!AreValid(userToken, gameId, out var user, out var game, out var error))
            return error;

        if (!game.Game.IsHost(user.Id))
            return Error(ErrorType.NoHost);
        
        game.Game.RemoveUser(userId, true);
        await Clients.Group(gameId).HandleRemoveUser(userId, true);
        await RemoveFromGroup(gameId, userId);
        
        await PersistGameIfNeeded(game);
        
        await NudgeBots(game);
        return Success();
    }
    
    public async Task<VoidResult> RequestUpdateSettings(string userToken, string gameId, GameSettings settings)
    {
        if (!AreValid(userToken, gameId, out var user, out var game, out var error))
            return error;
        
        if (game.CreatorUserId != user.Id)
            return Error(ErrorType.NoCreator);

        game.Game.Settings = settings;
        
        await Clients.Group(gameId).HandleUpdateSettings(settings);
        
        await PersistGameIfNeeded(game);
        
        return Success();
    }
    
    public async Task<Result<ServerStatus>> RequestCloseGame(string userToken, string gameId)
    {
        if (!AreValid<ServerStatus>(userToken, gameId, out var user, out var game, out var error))
            return error;
        
        if (game.CreatorUserId != user.Id)
            return Error<ServerStatus>(ErrorType.NoCreator);

        foreach (var userId in game.Game.Participation.PlayerNames.Keys)
        {
            game.Game.RemoveUser(userId, true);
            await Clients.Group(gameId).HandleRemoveUser(userId, true);
            await RemoveFromGroup(gameId, userId);
        }

        RunningGamesByGameId.Remove(gameId, out _);
        
        await EraseGame(game);
        
        UpdateServerStatusIfNeeded(true);
        return Success(FilteredServerStatus(GameListScope.Active, user.Id));
    }

    private async Task AddToGroup(string gameId, int userId, string connectionId)
    {
        await Groups.AddToGroupAsync(connectionId, gameId);
        if (!ConnectionInfoByUserId.TryGetValue(userId, out var value))
        {
            value = new UserConnections();
            ConnectionInfoByUserId[userId] = value;
        }

        value.SetConnectionId(gameId, Context.ConnectionId);
    }
    
    private async Task RemoveFromGroup(string gameId, int userId)
    {
        if (ConnectionInfoByUserId.TryGetValue(userId, out var connectionInfo) &&
            connectionInfo.TryGetConnectionId(gameId, out var connectionId))
        {
            connectionInfo.RemoveConnectionId(gameId);
            await Groups.RemoveFromGroupAsync(connectionId, gameId);
        }
    }

    public async Task<Result<GameInitInfo>> RequestObserveGame(string userToken, string gameId, string hashedPassword)
    {
        if (!AreValid<GameInitInfo>(userToken, gameId, out var user, out var game, out var error))
            return error;

        if (!game.Game.IsObserver(user.Id))
        {
            if (game.ObserversRequirePassword && !string.IsNullOrEmpty(game.HashedPassword) &&
                !game.HashedPassword.Equals(hashedPassword))
                return Error<GameInitInfo>(ErrorType.IncorrectGamePassword);

            if (game.Game.IsPlayer(user.Id))
                return Error<GameInitInfo>(ErrorType.AlreadyPlayer);

            game.Game.AddObserver(user.Id, user.PlayerName);
            await Clients.Group(gameId).HandleObserveGame(user.Id, user.PlayerName);
        }

        await AddToGroup(gameId, user.Id, Context.ConnectionId);
        
        await PersistGameIfNeeded(game);

        return Success(new GameInitInfo
        {
            GameId = gameId,
            GameState = GameState.GetStateAsString(game.Game),
            GameName = game.Name,
            Participation = game.Game.Participation
        });
    }

    public async Task<Result<GameInitInfo>> RequestReconnectGame(string userToken, string gameId)
    {
        if (!AreValid<GameInitInfo>(userToken, gameId, out var user, out var game, out var error))
            return error;
        
        if (!game.Game.IsParticipant(user.Id))
            return Error<GameInitInfo>(ErrorType.UserNotInGame);
       
        await AddToGroup(gameId, user.Id, Context.ConnectionId);
        
        await PersistGameIfNeeded(game);
        
        return Success(new GameInitInfo
        {
            GameId = gameId, 
            GameState = GameState.GetStateAsString(game.Game),
            Participation = game.Game.Participation
        } );
    }
    
    public async Task<Result<GameInitInfo>> RequestGameState(string userToken, string gameId)
    {
        if (!AreValid<GameInitInfo>(userToken, gameId, out var user, out var game, out var error))
            return error;

        if (!game.Game.IsParticipant(user.Id))
            return Error<GameInitInfo>(ErrorType.UserNotInGame);

        return await Task.FromResult(Success(new GameInitInfo
        {
            GameId = gameId, 
            GameState = GameState.GetStateAsString(game.Game), 
            GameName = game.Name,
            Participation = game.Game.Participation
        }));
    }

    public async Task<VoidResult> RequestSetSkin(string userToken, string gameId, string skin)
    {
        if (!AreValid(userToken, gameId, out var user, out var game, out var error))
            return error;

        if (!game.Game.IsHost(user.Id))
            return Error(ErrorType.NoHost);

        await Clients.Group(gameId).HandleSetSkin(skin);
        
        return Success();
    }

    public async Task<VoidResult> RequestUndo(string userToken, string gameId, int untilEventNr)
    {
        if (!AreValid(userToken, gameId, out var user, out var game, out var error))
            return error;

        if (!game.Game.IsHost(user.Id))
            return Error(ErrorType.NoHost);

        game.Game = game.Game.Undo(untilEventNr);
        await Clients.Group(gameId).HandleUndo(untilEventNr);

        await PersistGameIfNeeded(game);
        
        await NudgeBots(game);
        return Success();
    }

    public async Task<VoidResult> RequestPauseBots(string userToken, string gameId)
    {
        if (!AreValid(userToken, gameId, out var user, out var game, out var error))
            return error;

        if (!game.Game.IsHost(user.Id))
            return Error(ErrorType.NoHost);

        game.Game.Participation.BotsArePaused = !game.Game.Participation.BotsArePaused;
        await Clients.All.HandleBotStatus(game.Game.Participation.BotsArePaused);
        
        await PersistGameIfNeeded(game);

        await NudgeBots(game);
        return Success();
    }

    public async Task<Result<ServerStatus>> RequestHeartbeat(string userToken, GameListScope scope)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var loggedInUser))
            return Error<ServerStatus>(ErrorType.UserNotFound);

        await RunTasksIfNeeded();

        loggedInUser.LastSeenDateTime = DateTime.Now;

        return Success(FilteredServerStatus(scope, loggedInUser.Id));
    }

    private static ServerStatus FilteredServerStatus(GameListScope scope, int userId)
    {
        switch (scope)
        {
            case GameListScope.Active:
            {
                var lastActiveGameThreshold = DateTimeOffset.Now.AddMinutes(-ActiveGameThresholdMinutes);
                var filteredServerStatus = new ServerStatus
                {
                    RunningGames = RunningGames.Where(mg => mg.LastActivity >= lastActiveGameThreshold).ToArray(),
                    OwnGames = RunningGames.Where(mg => mg.CreatorId == userId).ToArray(),
                    ScheduledGames = ScheduledGames,
                    RecentlySeenUsers = RecentlySeenUsers,
                };
                return filteredServerStatus;
            }
            
            case GameListScope.All:
            {
                var completeServerStatus = new ServerStatus
                {
                    RunningGames = RunningGames,
                    OwnGames = RunningGames.Where(mg => mg.CreatorId == userId).ToArray(),
                    ScheduledGames = ScheduledGames,
                    RecentlySeenUsers = RecentlySeenUsers,
                };
                return completeServerStatus;
            }

            case GameListScope.None:
            default:
            {
                var minimalServerStatus = new ServerStatus
                {
                    RunningGames = [],
                    OwnGames = [],
                    ScheduledGames = [],
                    RecentlySeenUsers = RecentlySeenUsers,
                };
            
                return minimalServerStatus;
            }
        }
    }

    private async Task RunTasksIfNeeded()
    {
        await RestoreGamesIfServerJustStarted();
        CleanupScheduledGamesIfNeeded();
        await PersistScheduledGamesIfNeeded();
        await CleanupUserTokensIfNeeded();
        UpdateServerStatusIfNeeded();
    }

    private static void UpdateServerStatusIfNeeded(bool forceUpdate = false)
    {
        var now = DateTimeOffset.Now;
        
        if (!forceUpdate && LastUpdatedServerStatus.AddMilliseconds(ServerStatusFrequencyMs) > now)
            return;

        RunningGames = RunningGamesByGameId.Values.Select(ExtractGameInfo).ToArray();

        ScheduledGames = ScheduledGamesByGameId.Values.Select(ExtractScheduledGameInfo).ToArray();

        RecentlySeenUsers = UsersByUserToken
            .Where(u => u.Value.LastSeenDateTime.AddMinutes(1) > now)
            .Select(u => new LoggedInUserInfo
            {
                Id = u.Value.Id,
                Status = u.Value.Status,
                Name = u.Value.User.PlayerName,
                LastSeen = u.Value.LastSeenDateTime,
            })
            .ToArray();
        
        LastUpdatedServerStatus = DateTimeOffset.Now;
    }

    private static ScheduledGameInfo ExtractScheduledGameInfo(ScheduledGame g) => new()
    {
        ScheduledGameId = g.ScheduledGameId,
        Ruleset = g.Ruleset,
        AsyncPlay = g.AsyncPlay,
        DateTime = g.DateTime,
        CreatorUserId = g.CreatorUserId,
        CreatorName = g.CreatorPlayerName,
        NumberOfPlayers = g.NumberOfPlayers,
        MaximumTurns = g.MaximumTurns,
        Subscribers = g.SubscribedUsers,
        SubscriberNames = g.SubscribedUsers.Keys.ToDictionary(x => x, x => UsersById.TryGetValue(x, out var user) ? user.Name : "?"),
        AllowedFactionsInPlay = g.AllowedFactionsInPlay
    };
    

    private async Task SendEndOfGameMail(ManagedGame game)
    {
        var info = ExtractGameInfo(game);
        var state = GameState.GetStateAsString(game.Game);
        
        var from = Configuration["GameEndEmailFrom"];
        var to = Configuration["GameEndEmailTo"];
        if (from == null || to == null)
            return;
        
        var subject = $"{info.Name} ({info.NrOfPlayers} Players, {info.NrOfBots} Bots, Turn {info.Turn} - {info.Ruleset})";
        var saveGameToAttach = new Attachment(GenerateStreamFromString(state), "savegame" + DateTime.Now.ToString("yyyyMMdd.HHmm") + ".json");

        var sb = new StringBuilder();
        sb.AppendLine("<h1>Player table in HTML format</h1>");
        sb.AppendLine("<table>");
        sb.AppendLine("<thead>");
        sb.AppendLine("<tr><th>Faction</th><th>Seat</th><th>UserId</th>");
        sb.AppendLine("</thead>");
        sb.AppendLine("<tbody>");
        foreach (var player in game.Game.Players)
        {
            sb.AppendLine($"<tr><td>{DefaultSkin.Default.Describe(player.Faction)}</td><td>{player.Seat}</td><td>{game.Game.UserIdInSeat(player.Seat)}</td></tr>");
        }
        sb.AppendLine("</tbody>");
        sb.AppendLine("</table>");
        sb.AppendLine("<h1>Player table in CSV format</h1>");
        sb.AppendLine("<p>");
        sb.AppendLine("Faction;Seat;UserId");
        foreach (var player in game.Game.Players)
        {
            sb.AppendLine($"{DefaultSkin.Default.Describe(player.Faction)};{player.Seat};{game.Game.UserIdInSeat(player.Seat)}");
        }
        sb.AppendLine("</p>");
        
        MailMessage mailMessage = new()
        {
            From = new MailAddress(from),
            Subject = subject,
            IsBodyHtml = true,
            Body = sb.ToString(),
            Priority = info.NrOfBots < 0.5f * info.NrOfPlayers ? MailPriority.Normal : MailPriority.Low
        };

        mailMessage.To.Add(new MailAddress(to));
        mailMessage.Attachments.Add(saveGameToAttach);

        await SendMail(mailMessage);
    }
    
    private static async Task SendGameStatistics(Game game)
    {
        try
        {
            var statistics = GameStatistics.GetStatistics(game);
            var httpClient = new HttpClient();
            var data = GetStatisticsAsString(statistics);
            var json = new StringContent(data, Encoding.UTF8, "application/json");
            await httpClient.PostAsync("https://dune.games/.netlify/functions/plays-add", json);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error sending statistics: {0}", e.Message);
        }
    }

    private static string GetStatisticsAsString(GameStatistics g) => Utilities.Serialize(g);
    
    private static GameInfo ExtractGameInfo(ManagedGame managedGame) => new()
    {
        GameId = managedGame.GameId,
        CreatorId = managedGame.CreatorUserId,
        FactionsInPlay = managedGame.Game.CurrentPhase <= Phase.AwaitingPlayers ? 
            managedGame.Game.Settings.AllowedFactionsInPlay.ToArray() : 
            managedGame.Game.Players.Where(p => p.Faction != Faction.None).Select(p => p.Faction).ToArray(),
        NrOfBots = managedGame.Game.NumberOfBots,
        Ruleset = managedGame.Game.CurrentPhase <= Phase.AwaitingPlayers ? 
            Game.DetermineApproximateRuleset(managedGame.Game.Settings.AllowedFactionsInPlay, managedGame.Game.Settings.InitialRules, Game.ExpansionLevel)  : 
            Game.DetermineApproximateRuleset(managedGame.Game.Players.Select(p => p.Faction).ToList(), managedGame.Game.Rules, Game.ExpansionLevel),
        LastActivity = managedGame.LastActivity,
        MainPhase = managedGame.Game.CurrentMainPhase,
        Phase = managedGame.Game.CurrentPhase,
        Turn = managedGame.Game.CurrentTurn,
        Name = managedGame.Name,
        HasPassword = !string.IsNullOrEmpty(managedGame.HashedPassword),
        MaxPlayers = managedGame.Game.Settings.NumberOfPlayers,
        MaxTurns = managedGame.Game.Settings.MaximumTurns,
        NrOfPlayers = managedGame.Game.Participation.SeatedPlayers.Count,
        SeatedPlayers = managedGame.Game.Participation.SeatedPlayers,
        AvailableSeats = managedGame.Game.Players
            .Where(p => managedGame.Game.SeatIsAvailable(p.Seat))
            .Select(p => new AvailableSeatInfo
            {
                Seat = p.Seat, 
                Faction = p.Faction, 
                IsBot = p.IsBot
            }).ToArray()
    };
}