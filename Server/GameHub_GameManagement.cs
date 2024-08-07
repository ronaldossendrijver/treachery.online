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
            return Error<GameInitInfo>("User not found");
        
        if (GamesByGameToken.Values.Count(g => g.CreatorUserId == user.Id) >= 10)
            return Error<GameInitInfo>("You cannot have more than 10 active games");

        Game game;
        var initialParticipation = new GameParticipation();
        if (!string.IsNullOrEmpty(stateData))
        {
            var state = GameState.Load(stateData);
            var errorMessage = Game.TryLoad(state, initialParticipation, false, false, out var loadedGame);

            if (errorMessage == null)
                game = loadedGame;
            else
                return Error<GameInitInfo>(errorMessage.ToString());
        }
        else
        {
            game = new Game();
        }

        var gameId = Guid.NewGuid().ToString();
        var managedGame = new ManagedGame
        {
            CreatorUserId = user.Id,
            GameId = gameId,
            Game = game,
            HashedPassword = hashedPassword,
            ObserversRequirePassword = false,
            GameName = $"{user.PlayerName}'s Game"
        };
        
        var gameToken = GenerateToken();
        GamesByGameToken[gameToken] = managedGame; 
        GameTokensByGameId[gameId] = gameToken;

        await AddToGroup(gameToken, user.Id, Context.ConnectionId);
        game.AddPlayer(user.Id, user.PlayerName);
        game.SetOrUnsetHost(user.Id);
        
        if (!string.IsNullOrEmpty(skin))
            await Clients.Group(gameToken).HandleSetSkin(skin);
        
        return Success(new GameInitInfo
        {
            GameToken = gameToken, 
            GameState = stateData ?? GameState.GetStateAsString(game), 
            Participation = game.Participation
        });
    }
    
    public async Task<VoidResult> RequestLoadGame(string userToken, string gameToken, string stateData, string skin)
    {
        if (!AreValid(userToken, gameToken, out var user, out var managedGame, out var error))
            return error;
        
        var state = GameState.Load(stateData);
        var errorMessage = Game.TryLoad(state, managedGame.Game.Participation, false, false, out var loadedGame);

        if (errorMessage != null)
            return Error(errorMessage.ToString());
            
        managedGame.Game = loadedGame;
        await Clients.Group(gameToken).HandleLoadGame(new GameInitInfo
        {
            GameToken = gameToken, 
            GameState = stateData, 
            Participation = managedGame.Game.Participation
        });

        if (!string.IsNullOrEmpty(skin))
            await Clients.Group(gameToken).HandleSetSkin(skin);
        
        return Success();
    }
    
    public async Task<Result<GameInitInfo>> RequestJoinGame(string userToken, string gameId, string hashedPassword, int seat)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user))
            return Error<GameInitInfo>("User not found");

        if (!GameTokensByGameId.TryGetValue(gameId, out var gameToken) || !GamesByGameToken.TryGetValue(gameToken, out var game))
            return Error<GameInitInfo>("Game not found");

        if (!game.Game.IsPlayer(user.Id))
        {
            if (!string.IsNullOrEmpty(game.HashedPassword) && !game.HashedPassword.Equals(hashedPassword))
                return Error<GameInitInfo>("Incorrect password");
        
            if (!game.Game.IsOpen(seat) || game.Game.WasKicked(user.Id))
                return Error<GameInitInfo>("Seat is not available");

            game.Game.AddPlayer(user.Id, user.PlayerName, seat);
        }
      
        await Clients.Group(gameToken).HandleJoinGame(user.Id, user.PlayerName, seat);
        await AddToGroup(gameToken, user.Id, Context.ConnectionId);
        return Success(new GameInitInfo
        {
            GameToken = gameToken, 
            GameState = GameState.GetStateAsString(game.Game), 
            Participation = game.Game.Participation
        });
    }

    public async Task<VoidResult> RequestOpenOrCloseSeat(string userToken, string gameToken, int seat)
    {
        if (!AreValid(userToken, gameToken, out var user, out var game, out var error))
            return error;
        
        if (!game.Game.IsHost(user.Id))
            return Error("You are not a host");

        game.Game.OpenOrCloseSeat(seat);
        await Clients.Group(gameToken).HandleOpenOrCloseSeat(seat);
        return Success();
    }
    
    public async Task<VoidResult> RequestSetOrUnsetHost(string userToken, string gameToken, int userId)
    {
        if (!AreValid(userToken, gameToken, out var user, out var game, out var error))
            return error;

        if (!game.Game.IsHost(user.Id))
            return Error("You are not a host");

        if (game.Game.IsHost(userId) && game.Game.NumberOfHosts <= 1) 
            return Error("You cannot remove the only remaining host from the game");
        
        game.Game.SetOrUnsetHost(userId);
        await Clients.Group(gameToken).HandleSetOrUnsetHost(userId);
        return Success();
    }

    public async Task<VoidResult> RequestLeaveGame(string userToken, string gameToken)
    {
        if (!AreValid(userToken, gameToken, out var user, out var game, out var error))
            return error;

        game.Game.RemoveUser(user.Id, false);
        await Clients.Group(gameToken).HandleRemoveUser(user.Id, false);
        await RemoveFromGroup(gameToken, user.Id);
        return Success();
    }

    public async Task<VoidResult> RequestKick(string userToken, string gameToken, int userId)
    {
        if (!AreValid(userToken, gameToken, out var user, out var game, out var error))
            return error;

        if (!game.Game.IsHost(user.Id))
            return Error("You are not a host");
        
        game.Game.RemoveUser(userId, true);
        await Clients.Group(gameToken).HandleRemoveUser(userId, true);
        await RemoveFromGroup(gameToken, userId);
        return Success();
    }
    
    public async Task<VoidResult> RequestCloseGame(string userToken, string gameId)
    {
        if (!GameTokensByGameId.TryGetValue(gameId, out var gameToken))
            return Error("Game not found");
        
        if (!AreValid(userToken, gameToken, out var user, out var game, out var error))
            return error;
        
        if (game.CreatorUserId != user.Id)
            return Error("You are not the creator of this game");

        foreach (var userId in game.Game.Participation.Users.Keys)
        {
            game.Game.RemoveUser(userId, true);
            await Clients.Group(gameToken).HandleRemoveUser(userId, true);
            await RemoveFromGroup(gameToken, userId);
        }

        GameTokensByGameId.Remove(game.GameId, out _);
        GamesByGameToken.Remove(gameToken, out _);
        
        return Success();
    }

    private async Task AddToGroup(string gameToken, int userId, string connectionId)
    {
        await Groups.AddToGroupAsync(connectionId, gameToken);
        if (!ConnectionInfoByUserId.ContainsKey(userId))
        {
            ConnectionInfoByUserId[userId] = new ConnectionInfo();
        }
        
        ConnectionInfoByUserId[userId].ConnectionIdByGameToken[gameToken] = Context.ConnectionId;
    }
    
    private async Task RemoveFromGroup(string gameToken, int userId)
    {
        if (ConnectionInfoByUserId.TryGetValue(userId, out var connectionInfo) &&
            connectionInfo.ConnectionIdByGameToken.TryGetValue(gameToken, out var connectionId))
        {
            await Groups.RemoveFromGroupAsync(connectionId, gameToken);
        }
    }

    public async Task<Result<GameInitInfo>> RequestObserveGame(string userToken, string gameId, string hashedPassword)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user))
            return Error<GameInitInfo>("User not found");

        if (!GameTokensByGameId.TryGetValue(gameId, out var gameToken) || !GamesByGameToken.TryGetValue(gameToken, out var game))
            return Error<GameInitInfo>("Game not found");
        
        if (game.ObserversRequirePassword && !string.IsNullOrEmpty(game.HashedPassword) && !game.HashedPassword.Equals(hashedPassword))
            return Error<GameInitInfo>("Incorrect password");
        
        await AddToGroup(gameToken, user.Id, Context.ConnectionId);
        game.Game.AddObserver(user.Id, user.PlayerName);
        await Clients.Group(gameToken).HandleObserveGame(user.Id, user.PlayerName);
        
        return Success(new GameInitInfo
        {
            GameToken = gameToken, 
            GameState = GameState.GetStateAsString(game.Game), 
            Participation = game.Game.Participation
        });
    }

    public async Task<Result<GameInitInfo>> RequestReconnectGame(string userToken, string gameToken)
    {
        if (!AreValid<GameInitInfo>(userToken, gameToken, out var user, out var game, out var error))
            return error;
        
        await AddToGroup(gameToken, user.Id, Context.ConnectionId);
        return Success(new GameInitInfo
        {
            GameToken = gameToken, 
            GameState = GameState.GetStateAsString(game.Game),
            Participation = game.Game.Participation
        } );
    }
    
    public async Task<Result<GameInitInfo>> RequestGameState(string userToken, string gameToken)
    {
        if (!AreValid<GameInitInfo>(userToken, gameToken, out _, out var game, out var error))
            return error;
        
        return await Task.FromResult(Success(new GameInitInfo
        {
            GameToken = gameToken, 
            GameState = GameState.GetStateAsString(game.Game), 
            Participation = game.Game.Participation
        }));
    }

    public async Task<VoidResult> RequestSetSkin(string userToken, string gameToken, string skin)
    {
        if (!AreValid(userToken, gameToken, out var user, out var game, out var error))
            return error;

        if (!game.Game.IsHost(user.Id))
            return Error("You are not a host");

        await Clients.Group(gameToken).HandleSetSkin(skin);
        return Success();
    }

    public async Task<VoidResult> RequestUndo(string userToken, string gameToken, int untilEventNr)
    {
        if (!AreValid(userToken, gameToken, out var user, out var game, out var error))
            return error;

        if (!game.Game.IsHost(user.Id))
            return Error("You are not a host");

        game.Game.Undo(untilEventNr);
        await Clients.Group(gameToken).HandleUndo(untilEventNr);
        return Success();
    }

    public async Task<VoidResult> RequestPauseBots(string userToken, string gameToken)
    {
        if (!AreValid(userToken, gameToken, out var user, out var game, out var error))
            return error;

        if (!game.Game.IsHost(user.Id))
            return Error("You are not the host");

        game.BotsArePaused = !game.BotsArePaused;
        await Clients.All.HandleBotStatus(game.BotsArePaused);

        if (!game.BotsArePaused)
        {
            await PerformBotEvent(gameToken, game);
        }
        
        return Success();
    }

    public async Task<Result<List<GameInfo>>> RequestRunningGames(string userToken)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user))
            return Error<List<GameInfo>>("User not found");

        var result = GamesByGameToken.Values.Select(g => GameInfo.Extract(g, user.Id)).ToList();
        return await Task.FromResult(Success(result));
    }
    
    private void SendEndOfGameMail(string content, GameInfo info)
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

        SendMail(mailMessage);
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