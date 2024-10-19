using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Treachery.Shared;

namespace Treachery.Server;

public partial class GameHub
{
    public async Task<Result<GameInitInfo>> RequestCreateGame(string userToken, string hashedPassword, string stateData, string skin)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user))
            return Error<GameInitInfo>(ErrorType.UserNotFound);
        
        if (GamesByGameId.Values.Count(g => g.CreatorUserId == user.Id) >= 10)
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
            HashedPassword = hashedPassword,
            ObserversRequirePassword = false
        };
        
        GamesByGameId[gameId] = managedGame;

        game.AddPlayer(user.Id, user.PlayerName);
        game.SetOrUnsetHost(user.Id);
        await AddToGroup(gameId, user.Id, Context.ConnectionId);
        
        if (!string.IsNullOrEmpty(skin))
            await Clients.Group(gameId).HandleSetSkin(skin);
        
        return Success(new GameInitInfo
        {
            GameId = gameId, 
            GameState = stateData ?? GameState.GetStateAsString(game), 
            Participation = game.Participation
        });
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
            Participation = game.Game.Participation
        });

        if (!string.IsNullOrEmpty(skin))
            await Clients.Group(gameId).HandleSetSkin(skin);
        
        if (!game.BotsArePaused)
        {
            await PerformBotEvent(gameId, game);
        }
        
        return Success();
    }

    public async Task<VoidResult> RequestAssignSeats(string userToken, string gameId, Dictionary<int, int> assignment)
    {
        if (!AreValid(userToken, gameId, out var user, out var game, out var error))
            return error;

        if (!game.Game.IsHost(user.Id))
            return Error(ErrorType.NoHost);

        game.Game.Participation.SeatedPlayers = assignment;
        await Clients.Group(gameId).HandleAssignSeats(assignment);

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
        
            if (!game.Game.IsOpen(seat) || game.Game.WasKicked(user.Id))
                return Error<GameInitInfo>(ErrorType.SeatNotAvailable);
            
            if (game.Game.IsObserver(user.Id))
                return Error<GameInitInfo>(ErrorType.AlreadyObserver);

            game.Game.AddPlayer(user.Id, user.PlayerName, seat);
            await Clients.Group(gameId).HandleJoinGame(user.Id, user.PlayerName, seat);
            
            if (game.Game.NumberOfPlayers == 1)
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
            Participation = game.Game.Participation
        });
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

        GamesByGameId.Remove(gameId, out _);
        
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

        game.Game.Undo(untilEventNr);
        await Clients.Group(gameId).HandleUndo(untilEventNr);
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

        if (!game.BotsArePaused)
        {
            await PerformBotEvent(gameId, game);
        }
        
        return Success();
    }

    public async Task<Result<List<GameInfo>>> RequestRunningGames(string userToken)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user))
            return Error<List<GameInfo>>(ErrorType.NoHost);

        var result = GamesByGameId.Values.Select(g => GameInfo.Extract(g, user.Id)).ToList();
        return await Task.FromResult(Success(result));
    }
    
    private async Task SendEndOfGameMail(string content, GameInfo info)
    {
        var from = configuration["GameEndEmailFrom"];
        var to = configuration["GameEndEmailTo"];
        if (from == null || to == null)
            return;
        
        var ruleset = Game.DetermineApproximateRuleset(info.FactionsInPlay, info.Rules, info.ExpansionLevel);
        var subject = $"{info.GameName} ({info.Players.Length} Players, {info.NumberOfBots} Bots, Turn {info.CurrentTurn} - {ruleset})";
        var saveGameToAttach = new Attachment(GenerateStreamFromString(content), "savegame" + DateTime.Now.ToString("yyyyMMdd.HHmm") + ".json");
        
        MailMessage mailMessage = new()
        {
            From = new MailAddress(from),
            Subject = subject,
            IsBodyHtml = true,
            Body = "Game finished!",
            Priority = info.NumberOfBots < 0.5f * info.Players.Length ? MailPriority.Normal : MailPriority.Low
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