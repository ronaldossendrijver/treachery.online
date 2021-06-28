/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Treachery.Shared;

namespace Treachery.Client
{
    public class Host
    {
        public const int MAX_HEARTBEATS = 28800;  //28800 heartbeats of 6 seconds each = 48 hours
        public const int HEARTBEAT_DELAY = 6000;

        public int HostID;
        public int GameID;
        private readonly HubConnection connection;
        private readonly string Name;
        private readonly string gamePassword;
        private readonly Handler h;
        private Game GameAtHost;
        public readonly string LoadedGameData;
        public readonly Game LoadedGame;

        public GameInfo GameBeingEstablished = new GameInfo()
        {
            MaximumNumberOfTurns = 10,
            MaximumNumberOfPlayers = 6,
            HostParticipates = true,
            Ruleset = Ruleset.BasicGame,
            Rules = new List<Rule>(),
            FactionsInPlay = new List<Faction> { Faction.Green, Faction.Black, Faction.Yellow, Faction.Red, Faction.Orange, Faction.Blue, Faction.Grey, Faction.Purple }
        };

        public Host(string hostName, string gamePassword, Handler h, string loadedGameData, Game loadedGame)
        {
            HostID = (new Random()).Next();
            Name = hostName;
            this.gamePassword = gamePassword;
            GameID = (new Random()).Next();
            this.h = h;
            LoadedGameData = loadedGameData;
            LoadedGame = loadedGame;
            connection = h._connection;
            GameAtHost = new Game();
            RegisterHandlers();
            _ = Heartbeat();
        }

        private void RegisterHandlers()
        {
            RegisterGameEventHandlers();

            connection.On<string, PlayerJoined>("ReceiveRequest_PlayerJoined", (playerConnectionID, e) => ReceiveRequest_PlayerJoined(playerConnectionID, e));
            connection.On<string, PlayerRejoined>("ReceiveRequest_PlayerRejoined", (playerConnectionID, e) => ReceiveRequest_PlayerRejoined(playerConnectionID, e));
            connection.On<string, ObserverJoined>("ReceiveRequest_ObserverJoined", (playerConnectionID, e) => ReceiveRequest_ObserverJoined(playerConnectionID, e));
            connection.On<string, ObserverRejoined>("ReceiveRequest_ObserverRejoined", (playerConnectionID, e) => ReceiveRequest_ObserverRejoined(playerConnectionID, e));

            connection.On<string>("ProcessHeartbeat", (playerName) => Receive_Heartbeat(playerName));
            connection.On<ChatMessage>("RequestChatMessage", (e) => ReceiveRequest_ChatMessage(e));
        }

        
        private void RegisterGameEventHandlers()
        {
            foreach (var t in Game.GetGameEventTypes())
            {
                RegisterGameEventHandler(t);
            }
        }

        private IDisposable RegisterGameEventHandler(Type t)
        {
            return connection.On("ReceiveRequest_" + t.Name, new Type[] { t }, ReceiveRequest_Event);
        }

        private int nrOfHeartbeats = 0;
        public async Task Heartbeat()
        {
            if (nrOfHeartbeats++ < MAX_HEARTBEATS)
            {
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
            }

            _ = Task.Delay(HEARTBEAT_DELAY).ContinueWith(e => Heartbeat());
        }

        private GameInfo CurrentGame
        {
            get
            {
                GameInfo result = null;

                if (GameAtHost.CurrentPhase == Phase.AwaitingPlayers)
                {
                    result = GameBeingEstablished;
                    result.Players = JoinedPlayers.Where(p => GameBeingEstablished.HostParticipates || p != h.PlayerName).ToArray();
                    result.NumberOfBots = 0;
                }
                else
                {
                    result = new GameInfo()
                    {
                        Players = GameAtHost.Players.Select(p => p.Name).ToArray(),
                        NumberOfBots = GameAtHost.Players.Count(p => p.IsBot),
                        Ruleset = GameAtHost.Ruleset,
                        Rules = GameAtHost.Rules.ToList()
                    };
                }

                result.HostID = HostID;
                result.GameName = Name + "'s Game";
                result.HasPassword = gamePassword != "";
                result.CurrentPhase = GameAtHost.CurrentPhase;
                result.CurrentMainPhase = GameAtHost.CurrentMainPhase;
                result.CurrentTurn = GameAtHost.CurrentTurn;

                return result;
            }
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
            if (GameAtHost.CurrentPhase == Phase.AwaitingPlayers)
            {
                foreach (var disconnectedPlayerName in DisconnectedPlayers.ToList())
                {
                    var thePlayer = JoinedPlayers.SingleOrDefault(p => p == disconnectedPlayerName);
                    if (thePlayer != null)
                    {
                        JoinedPlayers.Remove(thePlayer);
                        Heartbeats.Remove(disconnectedPlayerName);
                        h.RefreshControls();
                    }
                }
            }
        }

        private IEnumerable<string> DisconnectedPlayers
        {
            get
            {
                return Heartbeats.Where(kvp => kvp.Value.AddMilliseconds(1.5 * Handler.HEARTBEAT_DELAY) < DateTime.Now).Select(kvp => kvp.Key);
            }
        }

        //Processes that the given player is still connected
        public void Receive_Heartbeat(string playerName)
        {
            if (Heartbeats.ContainsKey(playerName))
            {
                Heartbeats.Remove(playerName);
            }

            Heartbeats.Add(playerName, DateTime.Now);
        }

        /* 
         * Approving Joining and Rejoining of games
         */

        public Dictionary<string, DateTime> Heartbeats { get; private set; } = new Dictionary<string, DateTime>();
        public List<string> JoinedPlayers { get; private set; } = new List<string>();
        private async Task ReceiveRequest_PlayerJoined(string playerConnectionId, PlayerJoined e)
        {
            try
            {
                string denyMessage = VerifyPlayerJoin(e.Name, e.HashedPassword);
                await connection.SendAsync("RespondPlayerJoined", GameID, playerConnectionId, HostID, denyMessage);

                if (denyMessage == "")
                {
                    JoinedPlayers.Add(e.Name);
                    h.RefreshControls();
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
                string denyMessage = VerifyPlayerRejoin(e.Name, e.HashedPassword);
                await connection.SendAsync("RespondPlayerRejoined", GameID, playerConnectionId, HostID, denyMessage);

                if (denyMessage == "")
                {
                    var data = GameState.GetStateAsString(h.Game);
                    var skin = Skin.Current.GetSkinAsString();
                    await connection.SendAsync("LoadGame", GameID, data, e.Name, skin);
                }
            }
            catch (Exception ex)
            {
                Support.Log(ex.ToString());
                Support.Log(ex.InnerException);
            }
        }

        public List<string> JoinedObservers = new List<string>();
        private async Task ReceiveRequest_ObserverJoined(string playerConnectionId, ObserverJoined e)
        {
            try
            {
                string denyMessage = "";
                await connection.SendAsync("RespondObserverJoined", GameID, playerConnectionId, HostID, denyMessage);
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
                string denyMessage = "";
                await connection.SendAsync("RespondObserverRejoined", GameID, playerConnectionId, HostID, denyMessage);
                if (!JoinedObservers.Contains(e.Name)) JoinedObservers.Add(e.Name);
                var data = GameState.GetStateAsString(h.Game);
                var skin = Skin.Current.GetSkinAsString();
                await connection.SendAsync("LoadGame", GameID, data, e.Name, skin);
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
            {
                return "Incorrect password.";
            }
            else if (GameAtHost.CurrentPhase != Phase.AwaitingPlayers)
            {
                return "Game is not available.";
            }
            else if (JoinedPlayers.Any(p => p.ToLower().Trim() == name.ToLower().Trim()))
            {
                return "There is already a player with the same name.";
            }

            return "";
        }

        private string VerifyPlayerRejoin(string name, string hashedPassword)
        {
            if (gamePassword != "" && !Support.VerifyHash(gamePassword, hashedPassword))
            {
                return "Incorrect password.";
            }
            else if (!GameAtHost.Players.Any(p => p.Name.ToLower().Trim() == name.ToLower().Trim()))
            {
                return "There is no player with that name.";
            }
            else if (!DisconnectedPlayers.Any(playername => playername.ToLower().Trim() == name.ToLower().Trim()))
            {
                return "No player with this name was disconnected.";
            }

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
                e.Game = GameAtHost;
                var result = e.Execute(true, true);
                if (result == "")
                {
                    await connection.SendAsync("NotifyUpdate", GameID, GameAtHost.EventCount, e);
                }
                else
                {
                    Support.Log(result);
                }
            }
            catch (Exception ex)
            {
                Support.Log(ex.ToString());
            }
        }



        private async Task ReceiveRequest_ChatMessage(ChatMessage e)
        {
            try
            {
                await connection.SendAsync("ApproveChatMessage", GameID, e);
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
                int undoNr = eventNr > 0 ? eventNr : GameAtHost.History.Count() - 1;
                await connection.SendAsync("Undo", GameID, undoNr);
                GameAtHost = GameAtHost.Undo(undoNr);
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
                GameAtHost = game;
                await connection.SendAsync("LoadGame", GameID, gameData, "", "");
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
                GameAtHost = game;
                await connection.SendAsync("LoadGame", GameID, gameData, "", skinData);
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
                await connection.SendAsync("LoadSkin", GameID, skinData);
            }
            catch (Exception ex)
            {
                Support.Log(ex.ToString());
            }
        }

        public async Task HandleGameFinished(Game game)
        {
            var nrOfHumans = game.Players.Count(p => !p.IsBot);
            var nrOfBots = game.Players.Count(p => p.IsBot);

            if (nrOfHumans > nrOfBots && nrOfHumans > 1)
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
    }
}
