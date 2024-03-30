/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Treachery.Client;

public class Host : IDisposable
{
    public const int MAX_HEARTBEATS = 28800;  //28800 heartbeats of 6 seconds each = 48 hours
    public const int HEARTBEAT_DELAY = 6000;

    public int HostID { get; }
    public string LoadedGameData { get; }
    public Game LoadedGame { get; }

    private readonly HubConnection connection;
    private readonly string Name;
    private readonly Client client;
    private readonly string gamePassword;
    private readonly int gameID;
    private Game gameAtHost;
    private bool stopped;

    public GameInfo GameBeingEstablished;

    public Host(string hostName, string gamePassword, Client client, string loadedGameData, Game loadedGame, HubConnection hubConnection)
    {
        HostID = new Random().Next();
        Name = hostName;
        this.gamePassword = gamePassword;
        gameID = new Random().Next();
        this.client = client;
        LoadedGameData = loadedGameData;
        LoadedGame = loadedGame;
        connection = hubConnection;
        gameAtHost = new Game();

        GameBeingEstablished = new GameInfo
        {
            MaximumNumberOfTurns = 10,
            MaximumNumberOfPlayers = 6,
            HostParticipates = true,
            ExpansionLevel = Game.ExpansionLevel,
            Rules = new List<Rule> { Rule.FillWithBots },
            FactionsInPlay = EstablishPlayers.AvailableFactions().ToList()
        };

        RegisterHandlers();
        stopped = false;
        _ = Heartbeat();
    }

    private readonly List<IDisposable> _registeredHandlers = new();
    private void RegisterHandlers()
    {
        foreach (var t in Game.GetGameEventTypes()) _registeredHandlers.Add(RegisterGameEventHandler(t));

        _registeredHandlers.Add(connection.On<string, PlayerJoined>("ReceiveRequest_PlayerJoined", (playerConnectionID, e) => ReceiveRequest_PlayerJoined(playerConnectionID, e)));
        _registeredHandlers.Add(connection.On<string, PlayerRejoined>("ReceiveRequest_PlayerRejoined", (playerConnectionID, e) => ReceiveRequest_PlayerRejoined(playerConnectionID, e)));
        _registeredHandlers.Add(connection.On<string, ObserverJoined>("ReceiveRequest_ObserverJoined", (playerConnectionID, e) => ReceiveRequest_ObserverJoined(playerConnectionID, e)));
        _registeredHandlers.Add(connection.On<string, ObserverRejoined>("ReceiveRequest_ObserverRejoined", (playerConnectionID, e) => ReceiveRequest_ObserverRejoined(playerConnectionID, e)));

        _registeredHandlers.Add(connection.On<string>("ProcessHeartbeat", playerName => Receive_Heartbeat(playerName)));
        _registeredHandlers.Add(connection.On<GameChatMessage>("RequestChatMessage", e => ReceiveRequest_ChatMessage(e)));
    }

    private void UnregisterHandlers()
    {
        foreach (var registeredHandler in _registeredHandlers) registeredHandler.Dispose();

        _registeredHandlers.Clear();
    }


    private IDisposable RegisterGameEventHandler(Type t)
    {
        return connection.On("ReceiveRequest_" + t.Name, new[] { t }, ReceiveRequest_Event);
    }


    public void Stop()
    {
        stopped = true;
        UnregisterHandlers();
    }

    private int nrOfHeartbeats;
    public async Task Heartbeat()
    {
        if (nrOfHeartbeats++ < MAX_HEARTBEATS && !stopped)
            try
            {
                if (connection.State == HubConnectionState.Connected)
                {
                    await AnnounceGame(CurrentGame);
                    CheckDisconnectedPlayers();
                }
            }
            catch (Exception e)
            {
                Support.Log(e.ToString());
            }

        _ = Task.Delay(HEARTBEAT_DELAY).ContinueWith(e => Heartbeat());
    }

    private GameInfo CurrentGame
    {
        get
        {
            GameInfo result = null;

            if (gameAtHost.CurrentPhase == Phase.AwaitingPlayers)
            {
                result = GameBeingEstablished;
                result.Players = JoinedPlayers.Where(p => GameBeingEstablished.HostParticipates || p != client.PlayerName).ToArray();
                result.NumberOfBots = 0;
            }
            else
            {
                result = new GameInfo
                {
                    Players = gameAtHost.Players.Select(p => p.Name).ToArray(),
                    FactionsInPlay = gameAtHost.Players.Select(p => p.Faction).ToList(),
                    NumberOfBots = gameAtHost.Players.Count(p => p.IsBot),
                    Rules = gameAtHost.Rules.ToList(),
                    LastAction = gameAtHost.History.Last().Time
                };
            }

            result.HostID = HostID;
            result.GameName = Name + "'s Game";
            result.HasPassword = gamePassword != "";
            result.CurrentPhase = gameAtHost.CurrentPhase;
            result.CurrentMainPhase = gameAtHost.CurrentMainPhase;
            result.CurrentTurn = gameAtHost.CurrentTurn;

            return result;
        }
    }

    public async Task JoinGame(string playerName)
    {
        await client.Request(HostID, new PlayerJoined { HashedPassword = Support.GetHash(gamePassword), Name = playerName });
    }

    //Broadcasts the game hosted by this client to everyone connected to treachery.online
    public async Task AnnounceGame(GameInfo info)
    {
        try
        {
            await connection.SendAsync("AnnounceGame", info);
        }
        catch (Exception e)
        {
            Support.Log(e.ToString());
            Support.Log(e.InnerException);
        }
    }

    private void CheckDisconnectedPlayers()
    {
        if (gameAtHost.CurrentPhase == Phase.AwaitingPlayers)
            foreach (var disconnectedPlayerName in DisconnectedPlayers.ToList())
            {
                var thePlayer = JoinedPlayers.SingleOrDefault(p => p == disconnectedPlayerName);
                if (thePlayer != null)
                {
                    JoinedPlayers.Remove(thePlayer);
                    Heartbeats.Remove(disconnectedPlayerName);
                    client.Refresh();
                }
            }
    }

    private IEnumerable<string> DisconnectedPlayers => Heartbeats.Where(kvp => kvp.Value.AddMilliseconds(1.5 * Client.HEARTBEAT_DELAY) < DateTime.Now).Select(kvp => kvp.Key);

    //Processes that the given player is still connected
    public void Receive_Heartbeat(string playerName)
    {
        if (Heartbeats.ContainsKey(playerName)) Heartbeats.Remove(playerName);

        Heartbeats.Add(playerName, DateTime.Now);
    }

    /*
     * Approving Joining and Rejoining of games
     */

    public Dictionary<string, DateTime> Heartbeats { get; } = new();
    public List<string> JoinedPlayers { get; } = new();
    private async Task ReceiveRequest_PlayerJoined(string playerConnectionId, PlayerJoined e)
    {
        try
        {
            var denyMessage = VerifyPlayerJoin(e.Name, e.HashedPassword);
            await connection.SendAsync("RespondPlayerJoined", gameID, playerConnectionId, HostID, denyMessage);

            if (denyMessage == "")
            {
                JoinedPlayers.Add(e.Name);
                client.Refresh();
            }
        }
        catch (Exception ex)
        {
            Support.Log(ex.ToString());
            Support.Log(ex.InnerException);
        }
    }

    private async Task ReceiveRequest_PlayerRejoined(string playerConnectionId, PlayerRejoined e)
    {
        try
        {
            var denyMessage = VerifyPlayerRejoin(e.Name, e.HashedPassword);
            await connection.SendAsync("RespondPlayerRejoined", gameID, playerConnectionId, HostID, denyMessage);

            if (denyMessage == "")
            {
                var data = GameState.GetStateAsString(client.Game);
                var skin = Skin.Current.SkinToString();
                await connection.SendAsync("LoadGame", gameID, data, e.Name, skin);
            }
        }
        catch (Exception ex)
        {
            Support.Log(ex.ToString());
            Support.Log(ex.InnerException);
        }
    }

    public List<string> JoinedObservers = new();
    private async Task ReceiveRequest_ObserverJoined(string playerConnectionId, ObserverJoined e)
    {
        try
        {
            var denyMessage = "";
            await connection.SendAsync("RespondObserverJoined", gameID, playerConnectionId, HostID, denyMessage);
            if (!JoinedObservers.Contains(e.Name)) JoinedObservers.Add(e.Name);
        }
        catch (Exception ex)
        {
            Support.Log(ex.ToString());
            Support.Log(ex.InnerException);
        }
    }

    private async Task ReceiveRequest_ObserverRejoined(string playerConnectionId, ObserverRejoined e)
    {
        try
        {
            var denyMessage = "";
            await connection.SendAsync("RespondObserverRejoined", gameID, playerConnectionId, HostID, denyMessage);
            if (!JoinedObservers.Contains(e.Name)) JoinedObservers.Add(e.Name);
            var data = GameState.GetStateAsString(client.Game);
            var skin = Skin.Current.SkinToString();
            await connection.SendAsync("LoadGame", gameID, data, e.Name, skin);
        }
        catch (Exception ex)
        {
            Support.Log(ex.ToString());
            Support.Log(ex.InnerException);
        }
    }

    private string VerifyPlayerJoin(string name, string hashedPassword)
    {
        if (gamePassword != "" && !Support.VerifyHash(gamePassword, hashedPassword))
            return "Incorrect password.";
        if (gameAtHost.CurrentPhase != Phase.AwaitingPlayers)
            return "Game is not available.";
        if (JoinedPlayers.Any(p => p.ToLower().Trim() == name.ToLower().Trim())) return "There is already a player with the same name.";

        return "";
    }

    private string VerifyPlayerRejoin(string name, string hashedPassword)
    {
        if (gamePassword != "" && !Support.VerifyHash(gamePassword, hashedPassword))
            return "Incorrect password.";
        if (!gameAtHost.Players.Any(p => p.Name.ToLower().Trim() == name.ToLower().Trim()))
            return "There is no player with that name.";
        if (!DisconnectedPlayers.Any(playername => playername.ToLower().Trim() == name.ToLower().Trim())) return "No player with this name was disconnected.";

        return "";
    }

    private async Task ReceiveRequest_Event(object[] e)
    {
        await ReceiveRequest_Event((GameEvent)e[0]);
    }

    //Approves of an event
    private async Task ReceiveRequest_Event(GameEvent e)
    {
        try
        {
            e.Initialize(gameAtHost);
            e.Time = DateTime.Now;
            var result = e.Execute(true, true);
            if (result == null)
                await connection.SendAsync("NotifyUpdate", gameID, gameAtHost.EventCount, e);
            else
                Support.Log(result.ToString(Skin.Current));
        }
        catch (Exception ex)
        {
            Support.Log(ex.ToString());
        }
    }



    private async Task ReceiveRequest_ChatMessage(GameChatMessage e)
    {
        try
        {
            await connection.SendAsync("ApproveChatMessage", gameID, e);
        }
        catch (Exception ex)
        {
            Support.Log(ex.ToString());
        }
    }

    public async Task Undo(int eventNr)
    {
        try
        {
            var undoNr = eventNr > 0 ? eventNr : gameAtHost.History.Count - 1;
            await connection.SendAsync("Undo", gameID, undoNr);
            gameAtHost = gameAtHost.Undo(undoNr);
        }
        catch (Exception ex)
        {
            Support.Log(ex.ToString());
        }
    }

    public async Task LoadGame(string gameData, Game game)
    {
        try
        {
            gameAtHost = game;
            await connection.SendAsync("LoadGame", gameID, gameData, "", "");
        }
        catch (Exception ex)
        {
            Support.Log(ex.ToString());
        }
    }

    public async Task LoadGameAndSkin(string gameData, Game game, string skinData)
    {
        try
        {
            gameAtHost = game;
            await connection.SendAsync("LoadGame", gameID, gameData, "", skinData);
        }
        catch (Exception ex)
        {
            Support.Log(ex.ToString());
        }
    }

    public async Task LoadSkin(string skinData)
    {
        try
        {
            await connection.SendAsync("LoadSkin", gameID, skinData);
        }
        catch (Exception ex)
        {
            Support.Log(ex.ToString());
        }
    }

    public async Task HandleGameFinished(Game game)
    {
        try
        {
            await connection.SendAsync("GameFinished", GameState.GetStateAsString(game), CurrentGame);
        }
        catch (Exception ex)
        {
            Support.Log(ex.ToString());
        }
    }

    public async Task UploadStatistics(Game game)
    {
        try
        {
            await connection.SendAsync("UploadStatistics", GameState.GetStateAsString(game));
        }
        catch (Exception ex)
        {
            Support.Log(ex.ToString());
        }
    }

    public async Task SetTimer(int value)
    {
        try
        {
            await connection.SendAsync("SetTimer", gameID, value);
        }
        catch (Exception ex)
        {
            Support.Log(ex.ToString());
        }
    }

    public void Dispose()
    {
        Stop();
    }
}