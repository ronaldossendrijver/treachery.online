using System.IO;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Treachery.Server;

public partial class GameHub
{
    public async Task<Result<GameInitInfo>> RequestCreateGame(string userToken, string hashedPassword, string stateData, string skin)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user))
            return Error<GameInitInfo>(ErrorType.UserNotFound);
        
        if (RunningGamesByGameId.Values.Count(g => g.CreatorUserId == user.Id) >= 10)
            return Error<GameInitInfo>(ErrorType.TooManyGames);

        Game game;
        var loadGame = !string.IsNullOrEmpty(stateData);
        if (loadGame)
        {
            var state = GameState.Load(stateData);
            var errorMessage = Game.TryLoad(state, new GameParticipation(), false, false, out var loadedGame);

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
            CreatorUserId = user.Id,
            GameId = gameId,
            Game = game,
            Name = $"{user.PlayerName}'s Game",
            HashedPassword = hashedPassword,
            ObserversRequirePassword = false
        };
        
        RunningGamesByGameId[gameId] = managedGame;

        game.AddPlayer(user.Id, user.PlayerName);
        game.SetOrUnsetHost(user.Id);
        await AddToGroup(gameId, user.Id, Context.ConnectionId);
        
        if (!string.IsNullOrEmpty(skin))
            await Clients.Group(gameId).HandleSetSkin(skin);
        
        return Success(new GameInitInfo
        {
            GameId = gameId, 
            GameState = stateData ?? GameState.GetStateAsString(game), 
            GameName = managedGame.Name,
            Participation = game.Participation
        });
    }

    public async Task<VoidResult> RequestScheduleGame(string userToken, DateTimeOffset dateTime, Ruleset? ruleset,
        int? numberOfPlayers, int? maximumTurns, List<Faction> allowedFactionsInPlay, bool asyncPlay)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user))
            return Error(ErrorType.UserNotFound);

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
        
        return await Task.FromResult(Success());
    }
    
    public async Task<VoidResult> RequestCancelGame(string userToken, string scheduledGameId)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user))
            return Error(ErrorType.UserNotFound);
        
        if (!ScheduledGamesByGameId.TryGetValue(scheduledGameId, out var scheduledGame)) 
            return Error(ErrorType.ScheduledGameNotFound);
        
        if (user.Id != scheduledGame.CreatorUserId)
            return Error(ErrorType.NoHost);

        ScheduledGamesByGameId.Remove(scheduledGameId, out _);
        
        return await Task.FromResult(Success());
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

        await NudgeBots(game);
        return Success();
    }

    private async Task NudgeBots(ManagedGame game)
    {
        if (!game.BotsArePaused)
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
            
            if (game.Game.IsObserver(user.Id))
                return Error<GameInitInfo>(ErrorType.AlreadyObserver);

            game.Game.AddPlayer(user.Id, user.PlayerName, seat);
            await Clients.Group(gameId).HandleJoinGame(user.Id, user.PlayerName, seat);
            
            if (game.Game.NumberOfPlayers == 1 || game.CreatorUserId == user.Id)
            {
                game.Game.SetOrUnsetHost(user.Id);
                await Clients.Group(gameId).HandleSetOrUnsetHost(user.Id);
            }
        }
      
        await AddToGroup(gameId, user.Id, Context.ConnectionId);
            
        return Success(new GameInitInfo
        {
            GameId = gameId, 
            GameState = GameState.GetStateAsString(game.Game), 
            GameName = game.Name,
            Participation = game.Game.Participation
        });
    }
    
    public async Task<VoidResult> RequestSubscribeGame(string userToken, string gameId, SubscriptionType subscription)
    {
        if (gameId == null)
            return Error(ErrorType.GameNotFound);
        
        if (!UsersByUserToken.TryGetValue(userToken, out var user))
        {
            return Error(ErrorType.UserNotFound);
        }
        
        if (!ScheduledGamesByGameId.TryGetValue(gameId, out var game))
        {
            return Error(ErrorType.GameNotFound);
        }

        game.SubscribedUsers[user.Id] = subscription;

        return await Task.FromResult(Success());
    }

    public async Task<VoidResult> RequestOpenOrCloseSeat(string userToken, string gameId, int seat)
    {
        if (!AreValid(userToken, gameId, out var user, out var game, out var error))
            return error;
        
        if (!game.Game.IsHost(user.Id))
            return Error(ErrorType.NoHost);

        game.Game.OpenOrCloseSeat(seat);
        await Clients.Group(gameId).HandleOpenOrCloseSeat(seat);
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
        return Success();
    }

    public async Task<VoidResult> RequestLeaveGame(string userToken, string gameId)
    {
        if (!AreValid(userToken, gameId, out var user, out var game, out var error))
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
        
        await NudgeBots(game);
        return Success();
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
        
        await NudgeBots(game);
        return Success();
    }
    
    public async Task<VoidResult> RequestCloseGame(string userToken, string gameId)
    {
        if (!AreValid(userToken, gameId, out var user, out var game, out var error))
            return error;
        
        if (game.CreatorUserId != user.Id)
            return Error(ErrorType.NoCreator);

        foreach (var userId in game.Game.Participation.Users.Keys)
        {
            game.Game.RemoveUser(userId, true);
            await Clients.Group(gameId).HandleRemoveUser(userId, true);
            await RemoveFromGroup(gameId, userId);
        }

        RunningGamesByGameId.Remove(gameId, out _);
        
        return Success();
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

        await NudgeBots(game);
        return Success();
    }

    public async Task<VoidResult> RequestPauseBots(string userToken, string gameId)
    {
        if (!AreValid(userToken, gameId, out var user, out var game, out var error))
            return error;

        if (!game.Game.IsHost(user.Id))
            return Error(ErrorType.NoHost);

        game.BotsArePaused = !game.BotsArePaused;
        await Clients.All.HandleBotStatus(game.BotsArePaused);

        await NudgeBots(game);
        return Success();
    }

    public async Task<Result<ServerStatus>> RequestHeartbeat(string userToken)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var loggedInUser))
            return Error<ServerStatus>(ErrorType.UserNotFound);

        loggedInUser.LastSeenDateTime = DateTime.Now;

        var lastSeenThreshold = DateTimeOffset.Now.AddSeconds(-30);
        var result = new ServerStatus
        {
            RunningGames = RunningGamesByGameId.Values.Select(g => Utilities.ExtractGameInfo(g, loggedInUser.Id)).ToList(),
            ScheduledGames = ScheduledGamesByGameId.Values.ToList(),
            LoggedInUsers = UsersByUserToken.Where(u => u.Value.LastSeenDateTime > lastSeenThreshold).Select(u => new LoggedInUserInfo
            {
                Id = u.Value.Id, 
                Status = u.Value.Status,
                PlayerName = u.Value.User.PlayerName, 
                LastSeen = u.Value.LastSeenDateTime,
            }).ToList()
        };

        await Task.CompletedTask;
        
        return Success(result);
    }
    
    private async Task SendEndOfGameMail(string content, GameInfo info)
    {
        var from = Configuration["GameEndEmailFrom"];
        var to = Configuration["GameEndEmailTo"];
        if (from == null || to == null)
            return;
        
        var subject = $"{info.GameName} ({info.ActualNumberOfPlayers} Players, {info.NumberOfBots} Bots, Turn {info.CurrentTurn} - {info.Ruleset})";
        var saveGameToAttach = new Attachment(GenerateStreamFromString(content), "savegame" + DateTime.Now.ToString("yyyyMMdd.HHmm") + ".json");
        
        MailMessage mailMessage = new()
        {
            From = new MailAddress(from),
            Subject = subject,
            IsBodyHtml = true,
            Body = "Game finished!",
            Priority = info.NumberOfBots < 0.5f * info.ActualNumberOfPlayers ? MailPriority.Normal : MailPriority.Low
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

    private static string GetStatisticsAsString(GameStatistics g)
    {
        var serializer = JsonSerializer.CreateDefault();
        serializer.TypeNameHandling = TypeNameHandling.None;
        var writer = new StringWriter();
        serializer.Serialize(writer, g);
        writer.Close();
        return writer.ToString();
    }
}