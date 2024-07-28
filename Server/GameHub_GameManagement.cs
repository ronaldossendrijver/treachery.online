﻿using System.IO;
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
    public async Task<Result<GameInitInfo>> RequestCreateGame(string userToken, string hashedPassword, string stateData = null)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user))
            return Error<GameInitInfo>("User not found");

        Game game;
        var initialParticipation = new GameParticipation();
        var initialSettings = new GameSettings();
        
        if (stateData != null)
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
        
        var managedGame = new ManagedGame
        {
            Game = game,
            Settings = initialSettings,
            HashedPassword = hashedPassword,
            ObserversRequirePassword = false
        };
        
        var gameToken = GenerateToken();
        var gameId = Guid.NewGuid().ToString();
        GamesByGameToken[gameToken] = managedGame; 
        GameTokensByGameId[gameId] = gameToken;

        await Groups.AddToGroupAsync(Context.ConnectionId, gameToken);

        game.AddPlayer(user.Id, user.PlayerName);
        await Clients.Group(gameToken).HandleJoinGame(user.Id, user.PlayerName);
        
        game.SetOrUnsetHost(user.Id);
        await Clients.Group(gameToken).HandleSetOrUnsetHost(user.Id);
        
        return Success(new GameInitInfo
        {
            GameToken = gameToken, 
            GameState = stateData ?? GameState.GetStateAsString(game), 
            Participation = game.Participation, 
            Settings = initialSettings
        });
        
    }
    
    public async Task<VoidResult> RequestLoadGame(string userToken, string gameToken, string stateData, string skin = null)
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
            Participation = managedGame.Game.Participation, 
            Settings = managedGame.Settings
        });

        if (skin != null)
            await Clients.Group(gameToken).HandleSetSkin(skin);
        
        return Success();
    }
    
    public async Task<Result<GameInitInfo>> RequestJoinGame(string userToken, string gameId, string hashedPassword, int seat)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user))
            return Error<GameInitInfo>("User not found");

        if (!GameTokensByGameId.TryGetValue(gameId, out var gameToken) || !GamesByGameToken.TryGetValue(gameToken, out var game))
            return Error<GameInitInfo>("Game not found");
        
        if (game.HashedPassword != null && !game.HashedPassword.Equals(hashedPassword))
            return Error<GameInitInfo>("Incorrect password");
        
        if (!game.Game.IsOpen(seat))
            return Error<GameInitInfo>("Seat is not available");
    
        await Groups.AddToGroupAsync(Context.ConnectionId, gameToken);
        game.Game.AddPlayer(user.Id, user.PlayerName, seat);
        await Clients.Group(gameToken).HandleJoinGame(user.Id, user.PlayerName, seat);
        return Success(new GameInitInfo
        {
            GameToken = gameToken, 
            GameState = GameState.GetStateAsString(game.Game), 
            Participation = game.Game.Participation,
            Settings = game.Settings
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

    public async Task<VoidResult> RequestSeatOrUnseatBot(string userToken, string gameToken, int seat)
    {
        if (!AreValid(userToken, gameToken, out var user, out var game, out var error))
            return error;

        if (!game.Game.IsHost(user.Id))
            return Error("You are not a host");

        game.Game.SeatOrUnseatBot(seat);
        await Clients.Group(gameToken).HandleSeatOrUnseatBot(seat);
        return Success();
    }

    public async Task<VoidResult> RequestLeaveGame(string userToken, string gameToken)
    {
        if (!AreValid(userToken, gameToken, out var user, out var game, out var error))
            return error;

        //TODO: remove from SignalR group?
        game.Game.RemoveUser(user.Id);
        await Clients.Group(gameToken).HandleRemoveUser(user.Id);
        return Success();
    }

    public async Task<VoidResult> RequestKick(string userToken, string gameToken, int userId)
    {
        if (!AreValid(userToken, gameToken, out var user, out var game, out var error))
            return error;

        if (!game.Game.IsHost(user.Id))
            return Error("You are not a host");
        
        //TODO: remove from SignalR group?
        game.Game.RemoveUser(userId);
        await Clients.Group(gameToken).HandleRemoveUser(userId);
        return Success();
    }

    public async Task<Result<GameInitInfo>> RequestObserveGame(string userToken, string gameId, string hashedPassword)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user))
            return Error<GameInitInfo>("User not found");

        if (!GameTokensByGameId.TryGetValue(gameId, out var gameToken) || !GamesByGameToken.TryGetValue(gameToken, out var game))
            return Error<GameInitInfo>("Game not found");
        
        if (game.ObserversRequirePassword && game.HashedPassword != null && !game.HashedPassword.Equals(hashedPassword))
            return Error<GameInitInfo>("Incorrect password");
        
        await Groups.AddToGroupAsync(Context.ConnectionId, gameToken);
        game.Game.AddObserver(user.Id, user.PlayerName);
        await Clients.Group(gameToken).HandleObserveGame(user.Id, user.PlayerName);
        
        return Success(new GameInitInfo
        {
            GameToken = gameToken, 
            GameState = GameState.GetStateAsString(game.Game), 
            Participation = game.Game.Participation,
            Settings = game.Settings
        });
    }

    public async Task<Result<GameInitInfo>> RequestReconnectGame(string userToken, string gameToken)
    {
        if (!AreValid<GameInitInfo>(userToken, gameToken, out _, out var game, out var error))
            return error;
        
        await Groups.AddToGroupAsync(Context.ConnectionId, gameToken);
        return Success(new GameInitInfo
        {
            GameToken = gameToken, 
            GameState = GameState.GetStateAsString(game.Game),
            Participation = game.Game.Participation,
            Settings = game.Settings
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
            Participation = game.Game.Participation,
            Settings = game.Settings
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
    
    public async Task<VoidResult> RequestRegisterHeartbeat(string userToken)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user))
            return Error("User not found");

        UserTokensLastSeen[userToken] = DateTime.Now;
        return await Task.FromResult(Success());
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
    
    private static GameInfo GetGameInfo(string gameId, ManagedGame managedGame) => new()
    {
        GameId = gameId,
        Players = managedGame.Game.PlayerNames.ToArray(),
        Observers = managedGame.Game.ObserverNames.ToArray(),
        FactionsInPlay = managedGame.Game.CurrentPhase <= Phase.SelectingFactions ? managedGame.Settings.FactionsInPlay : managedGame.Game.FactionsInPlay,
        NumberOfBots = managedGame.Game.NumberOfBots,
        Rules = managedGame.Game.CurrentPhase <= Phase.AwaitingPlayers ? managedGame.Settings.Rules.ToList() : managedGame.Game.Rules.ToList(),
        LastAction = managedGame.Game.CurrentPhase > Phase.AwaitingPlayers ? managedGame.Game.History.Last().Time : DateTime.Now,
        CurrentMainPhase = managedGame.Game.CurrentMainPhase,
        CurrentPhase = managedGame.Game.CurrentPhase,
        CurrentTurn = managedGame.Game.CurrentTurn,
        ExpansionLevel = Game.ExpansionLevel,
        GameName = managedGame.Game.Name,
        HasPassword = managedGame.HashedPassword != null,
        CreatorParticipates = true,
        InviteOthers = true,
        MaximumNumberOfPlayers = managedGame.Settings.MaximumNumberOfPlayers,
        MaximumNumberOfTurns = managedGame.Settings.MaximumTurns,
    };
}
