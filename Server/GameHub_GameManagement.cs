using System.IO;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Treachery.Shared;

namespace Treachery.Server;

public partial class GameHub
{
    public async Task<Result<JoinInfo>> RequestCreateGame(string userToken, string hashedPassword, string settings)
        => await CreateOrLoadGame(userToken, hashedPassword, settings, null);
    
    public async Task<Result<JoinInfo>> RequestLoadGame(string userToken, string hashedPassword, string settings, string stateData)
        => await CreateOrLoadGame(userToken, hashedPassword, settings, stateData);
    
    private async Task<Result<JoinInfo>> CreateOrLoadGame(string userToken, string hashedPassword, string settings, string stateData)
    {
        if (!usersByUserToken.TryGetValue(userToken, out var user))
            return Error<JoinInfo>("User not found");

        Game game;
        if (stateData != null)
        {
            var state = GameState.Load(stateData);
            var errorMessage = Game.TryLoad(state, false, false, out var loadedGame);

            if (errorMessage == null)
                game = loadedGame;
            else
                return Error<JoinInfo>(errorMessage.ToString());
        }
        else
        {
            game = new Game();
        }
        
        var managedGame = new ManagedGame
        {
            Game = game, //TODO apply settings here
            HashedPassword = hashedPassword
        };
        var gameToken = GenerateToken();
        var gameId = Guid.NewGuid().ToString();
        gamesByGameToken[gameToken] = managedGame; 
        gameTokensByGameId[gameId] = gameToken;

        await Groups.AddToGroupAsync(Context.ConnectionId, gameToken);

        game.AddPlayer(user.Id, user.PlayerName, -1);
        await Clients.Group(gameToken).HandleJoinGame(user.Id, user.PlayerName, -1);
        
        game.SetOrUnsetHost(user.Id);
        await Clients.Group(gameToken).HandleSetOrUnsetHost(user.Id);
        
        return await Task.FromResult(Success(new JoinInfo { GameToken = gameToken, GameState = stateData ?? GameState.GetStateAsString(game) }));
        
    }
    
    public async Task<Result<JoinInfo>> RequestJoinGame(string userToken, string gameId, string hashedPassword, int seat)
    {
        if (!usersByUserToken.TryGetValue(userToken, out var user))
            return Error<JoinInfo>("User not found");

        if (!gameTokensByGameId.TryGetValue(gameId, out var gameToken) || !gamesByGameToken.TryGetValue(gameToken, out var game))
            return Error<JoinInfo>("Game not found");
        
        if (game.HashedPassword != null && !game.HashedPassword.Equals(hashedPassword))
            return Error<JoinInfo>("Incorrect password");
        
        if (!game.Game.IsOpen(seat))
            return Error<JoinInfo>("Seat is not available");
    
        await Groups.AddToGroupAsync(Context.ConnectionId, gameToken);
        game.Game.AddPlayer(user.Id, user.PlayerName, seat);
        await Clients.Group(gameToken).HandleJoinGame(user.Id, user.PlayerName, seat);
        return Success(new JoinInfo { GameToken = gameToken, GameState = GameState.GetStateAsString(game.Game) });
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

    public async Task<Result<JoinInfo>> RequestObserveGame(string userToken, string gameId, string hashedPassword)
    {
        if (!usersByUserToken.TryGetValue(userToken, out var user))
            return Error<JoinInfo>("User not found");

        if (!gameTokensByGameId.TryGetValue(gameId, out var gameToken) || !gamesByGameToken.TryGetValue(gameToken, out var game))
            return Error<JoinInfo>("Game not found");
        
        if (game.ObserversRequirePassword && game.HashedPassword != null && !game.HashedPassword.Equals(hashedPassword))
            return Error<JoinInfo>("Incorrect password");
        
        await Groups.AddToGroupAsync(Context.ConnectionId, gameToken);
        game.Game.AddObserver(user.Id, user.PlayerName);
        await Clients.Group(gameToken).HandleObserveGame(user.Id, user.PlayerName);
        return Success(new JoinInfo { GameToken = gameToken, GameState = GameState.GetStateAsString(game.Game) });
    }

    public async Task<Result<JoinInfo>> RequestReconnectGame(string userToken, string gameToken)
    {
        if (!AreValid<JoinInfo>(userToken, gameToken, out _, out var game, out var error))
            return error;
        
        await Groups.AddToGroupAsync(Context.ConnectionId, gameToken);
        return Success(new JoinInfo { GameToken = gameToken, GameState = GameState.GetStateAsString(game.Game) } );
    }
    
    public async Task<Result<string>> RequestGameState(string userToken, string gameToken)
    {
        if (!AreValid<string>(userToken, gameToken, out _, out var game, out var error))
            return error;
        
        return await Task.FromResult(Success(GameState.GetStateAsString(game.Game)));
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
        Clients.All.HandleBotStatus(game.BotsArePaused);

        if (!game.BotsArePaused)
        {
            await PerformBotEvent(gameToken, game);
        }
        
        return Success();
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

