/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class PlayerSequence3
    {
        public IList<Player> Players { get; set; }

        public int RoundStartedAt { get; set; }

        private Game Game { get; set; }

        public int Current { get; set; }

        private int _direction = 1;

        private int _playerNumberInRound = 0;

        public PlayerSequence3(Game game, IEnumerable<Player> players)
        {
            Players = players.ToList();
            Game = game;
        }

        public void Start(Player p, bool ignorePlayersThatCantBid, int direction)
        {
            _playerNumberInRound = 0;
            _direction = direction;
            Current = p.PositionAtTable;
            RoundStartedAt = Current;
            NextPlayer(ignorePlayersThatCantBid);
            RoundStartedAt = Current;
        }

        public void Start(bool ignorePlayersThatCantBid, int direction)
        {
            _playerNumberInRound = 0;
            _direction = direction;
            var startLookingInSector = (int)Math.Ceiling((float)Game.SectorInStorm * Game.MaximumNumberOfPlayers / Map.NUMBER_OF_SECTORS) % Game.MaximumNumberOfPlayers;
            Current = FindNearestPlayerPosition(startLookingInSector, ignorePlayersThatCantBid);
            RoundStartedAt = Current;
        }

        public Player CurrentPlayer
        {
            get
            {
                if (_playerNumberInRound == 0 && Game.JuiceForcesFirstPlayer)
                {
                    return Game.CurrentJuice.Player;
                }
                else if (_playerNumberInRound == Players.Count - 1 && Game.JuiceForcesLastPlayer)
                {
                    return Game.CurrentJuice.Player;
                }
                else
                {
                    return Players.Where(p => p.PositionAtTable == Current).SingleOrDefault();
                }
            }
        }

        public Faction CurrentFaction => CurrentPlayer != null ? CurrentPlayer.Faction : Faction.None;

        public void NextRound(bool ignorePlayersThatCantBid)
        {
            _playerNumberInRound = 0;
            Current = FindNearestPlayerPosition(RoundStartedAt + _direction, ignorePlayersThatCantBid);
            RoundStartedAt = Current;
        }


        public void NextPlayer(bool ignorePlayersThatCantBid)
        {
            if (_playerNumberInRound == 0 && Game.JuiceForcesFirstPlayer)
            {

            }
            else if (_playerNumberInRound == Players.Count - 1 && Game.JuiceForcesLastPlayer)
            {

            }
            else
            {
                Current = FindNearestPlayerPosition(Current + _direction, ignorePlayersThatCantBid);
            }

            _playerNumberInRound = (_playerNumberInRound + 1) % Players.Count;
        }

        //Returns a position number at the table occupied by a player nearest to the indicated position. The number of positions is zero based and depends on the Maximum number of players selected at game start.
        private int FindNearestPlayerPosition(int positionToStartLooking, bool ignorePlayersThatCantBid)
        {
            int position = (Game.MaximumNumberOfPlayers + positionToStartLooking) % Game.MaximumNumberOfPlayers;
            for (int i = 0; i < Game.MaximumNumberOfPlayers; i++)
            {
                if (Players.Any(p => p.PositionAtTable == (position % Game.MaximumNumberOfPlayers) && (!ignorePlayersThatCantBid || p.HasRoomForCards) && !HasJuice(p)))
                {
                    return position;
                }
                else
                {
                    position = Mod(Game.MaximumNumberOfPlayers + position + _direction, Game.MaximumNumberOfPlayers);
                }
            }

            return -1;
        }

        private bool HasJuice(Player p)
        {
            return Game.CurrentJuice != null && Game.CurrentJuice.Player == p;
        }

        public override string ToString()
        {
            return string.Format(string.Join("->", Players.OrderBy(p => p.PositionAtTable).Select(p => string.Format("{0} ({1})", p.Name, p.PositionAtTable))) + ", Current: {0}", Current);
        }

        public IEnumerable<SequenceElement> GetPlayersInSequence()
        {
            var result = new List<SequenceElement>();

            if (Game.JuiceForcesFirstPlayer)
            {
                result.Add(new SequenceElement() { Player = Game.CurrentJuice?.Player, HasTurn = _playerNumberInRound == 0 });
            }

            for (int i = 0; i < Game.MaximumNumberOfPlayers; i++)
            {
                int pos = Mod(RoundStartedAt + _direction * i, Game.MaximumNumberOfPlayers);
                var playerAtPosition = Players.FirstOrDefault(p => p.PositionAtTable == pos);
                if (playerAtPosition != null && playerAtPosition != Game.CurrentJuice?.Player)
                {
                    bool hasTurn = (pos == Current && !(Game.JuiceForcesFirstPlayer && _playerNumberInRound == 0) && !(Game.JuiceForcesLastPlayer && _playerNumberInRound == Players.Count - 1));
                    result.Add(new SequenceElement() { Player = playerAtPosition, HasTurn = hasTurn });
                }
            }

            if (Game.JuiceForcesLastPlayer)
            {
                result.Add(new SequenceElement() { Player = Game.CurrentJuice.Player, HasTurn = _playerNumberInRound == Players.Count - 1 });
            }

            return result;
        }

        public static int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }
    }



    public class SequenceElement
    {
        public Player Player;
        public bool HasTurn;
    }

    public class PlayerSequence
    {
        private readonly List<Player> _played = new List<Player>();
        private readonly Game _game;
        private readonly bool _skipPlayersThatCantBidOnCards;
        private readonly int _direction;
        private Player _first;

        public PlayerSequence(Game game, bool skipPlayersThatCantBidOnCards, int direction, Player toStartWith)
        {
            _game = game;
            _skipPlayersThatCantBidOnCards = skipPlayersThatCantBidOnCards;
            _direction = direction;
            _first = toStartWith;

            Console.Write("Player sequence started: ");
            foreach (var player in PlayersInOrder)
            {
                Console.Write(" -> " + player.Faction + " (" + player.PositionAtTable + ")");
            }

            Console.WriteLine(". Storm at: " + _game.SectorInStorm + ", first player: " + _first + " (position: " + _first.PositionAtTable + "), current player: " + CurrentPlayer);
        }

        public PlayerSequence(Game game, bool skipPlayersThatCantBidOnCards, int direction)
            : this(game, skipPlayersThatCantBidOnCards, direction, DetermineFirstPlayer(game))
        {

        }

        public PlayerSequence(Game game, bool skipPlayersThatCantBidOnCards)
            : this(game, skipPlayersThatCantBidOnCards, 1, DetermineFirstPlayer(game))
        {

        }

        public PlayerSequence(Game game)
            : this(game, false, 1, DetermineFirstPlayer(game))
        {

        }

        public void Start()
        {
            _played.Clear();
        }

        private IEnumerable<Player> PlayersInOrder => _game.Players.Where(p => !_skipPlayersThatCantBidOnCards || p.HasRoomForCards).OrderBy(p => _direction * p.PositionAtTable).ToList();

        private Player PlayerAfter(Player currentPlayer)
        {
            Player result = null;
            Player first = null;
            bool currentPlayerFound = false;

            foreach (Player p in PlayersInOrder)
            {
                if (first == null) first = p;

                if (currentPlayerFound)
                {
                    result = p;
                }
                else if (p == currentPlayer)
                {
                    currentPlayerFound = true;
                }
            }

            if (result == null)
            {
                result = first;
            }

            return result;
        }

        public Faction CurrentFaction => CurrentPlayer.Faction;

        public Player CurrentPlayer
        {
            get
            {
                if (_game.JuiceForcesFirstPlayer && _played.Count == 0)
                {
                    return _game.CurrentJuice.Player;
                }
                else if (_game.JuiceForcesLastPlayer && PlayersInOrder.Count() == 1)
                {
                    return _game.CurrentJuice.Player;
                }
                else if (_played.Count == 0)
                {
                    if (_skipPlayersThatCantBidOnCards && !_first.HasRoomForCards)
                    {
                        return PlayerAfter(_first);
                    }
                    else
                    {
                        return _first;
                    }
                }
                else
                {
                    return PlayerAfter(_played[_played.Count - 1]);
                }
            }
        }

        public void NextPlayer()
        {
            _played.Add(CurrentPlayer);

            if (!PlayersInOrder.Any())
            {
                _played.Clear();
            }
        }

        public void NextRound()
        {
            _first = PlayerAfter(_played[0]);
            _played.Clear();
        }

        public IEnumerable<SequenceElement> GetPlayersInSequence()
        {
            var result = new List<SequenceElement>();

            Player added = _first;

            if (!_skipPlayersThatCantBidOnCards || _first.HasRoomForCards)
            {
                added = PlayerAfter(_first);
            }

            result.Add(new SequenceElement() { Player = added, HasTurn = CurrentPlayer == added });

            while (!result.Any(se => se.Player == PlayerAfter(added)))
            {
                added = PlayerAfter(added);
                result.Add(new SequenceElement() { Player = added, HasTurn = CurrentPlayer == added });
            }

            return result;
        }

        public static Player DetermineFirstPlayer(Game g)
        {
            var startLookingInSector = (int)Math.Ceiling((float)g.SectorInStorm * g.MaximumNumberOfPlayers / Map.NUMBER_OF_SECTORS) % g.MaximumNumberOfPlayers;

            Player result = null;

            int position = (g.MaximumNumberOfPlayers + startLookingInSector) % g.MaximumNumberOfPlayers;
            for (int i = 0; i < g.MaximumNumberOfPlayers; i++)
            {
                result = g.Players.FirstOrDefault(p => p.PositionAtTable == position);
                if (result != null)
                {
                    return result;
                }
                else
                {
                    position = Mod(g.MaximumNumberOfPlayers + position + 1, g.MaximumNumberOfPlayers);
                }
            }

            return null;
        }

        private static int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }
    }
}
