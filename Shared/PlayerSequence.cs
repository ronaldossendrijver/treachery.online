/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class PlayerSequence
    {
        private List<Player> Players { get; set; }
        private readonly List<Player> _played = new List<Player>();
        private readonly Game _game;
        private readonly bool _skipPlayersThatCantBidOnCards;
        private readonly int _direction;
        private int _round;

        public PlayerSequence(Game game, bool skipPlayersThatCantBidOnCards, int direction, Player toStartWith, bool beginNextToThatPlayer)
        {
            _game = game;
            _skipPlayersThatCantBidOnCards = skipPlayersThatCantBidOnCards;
            _direction = direction;
            _round = 1;

            Players = _game.Players.OrderBy(p => _direction * p.PositionAtTable).ToList();
            FirstPlayer = toStartWith;

            if (beginNextToThatPlayer && NumberOfPlayersThatMayGetTurn > 1)
            {
                FirstPlayer = PlayerAfter(FirstPlayer);
            }

            ChangeFirstPlayerIfTheyCantBid();

            if (_game.Version <= 117)
            {
                CurrentPlayer = DetermineCurrentPlayer();
            }
        }

        public PlayerSequence(Game game, bool skipPlayersThatCantBidOnCards, int direction)
            : this(game, skipPlayersThatCantBidOnCards, direction, DetermineFirstPlayer(game), false)
        {
        }

        public PlayerSequence(Game game)
            : this(game, false, 1, DetermineFirstPlayer(game), false)
        {
        }

        private void ChangeFirstPlayerIfTheyCantBid()
        {
            if (NumberOfPlayersThatMayGetTurn > 1)
            {
                while (!MayGetTurn(FirstPlayer) || JuiceForcesLast(FirstPlayer))
                {
                    FirstPlayer = PlayerAfter(FirstPlayer);
                }
            }
        }

        private int NumberOfPlayersThatMayGetTurn => Players.Count(p => !_skipPlayersThatCantBidOnCards || p.HasRoomForCards);

        private bool MayGetTurn(Player p) => !_skipPlayersThatCantBidOnCards || p.HasRoomForCards;

        private bool JuiceForcesLast(Player p) => _game.CurrentJuice != null && _game.CurrentJuice.Type == JuiceType.GoLast && _game.CurrentJuice.Player == p;

        public void CheckCurrentPlayer()
        {
            if (_round == 1 && !_played.Any())
            {
                ChangeFirstPlayerIfTheyCantBid();
            }

            CurrentPlayer = DetermineCurrentPlayer();
        }

        private Player PlayerAfter(Player currentPlayer, bool includePlayersThatCantBid = false)
        {
            bool currentPlayerFound = false;

            Player firstFound = null;
            foreach (Player p in Players)
            {
                if (currentPlayerFound)
                {
                    if (includePlayersThatCantBid || MayGetTurn(p))
                    {
                        return p;
                    }
                }
                else if (p == currentPlayer)
                {
                    currentPlayerFound = true;
                }

                if (firstFound == null && (includePlayersThatCantBid || MayGetTurn(p)))
                {
                    firstFound = p;
                }
            }

            return firstFound;
        }

        public Player FirstPlayer { get; private set; }

        public Faction CurrentFaction => CurrentPlayer != null ? CurrentPlayer.Faction : Faction.None;

        private Player _currentPlayer;

        public Player CurrentPlayer
        {
            get
            {
                if (_game.Version > 117)
                {
                    return DetermineCurrentPlayer();
                }
                else
                {
                    return _currentPlayer;
                }
            }

            private set
            {
                _currentPlayer = value;
            }
        }

        private Player DetermineCurrentPlayer()
        {
            if (_played.Count > 0)
            {
                bool found = false;
                foreach (var p in PlayersInOrder)
                {
                    if (found)
                    {
                        return p;
                    }
                    else
                    {
                        found = p == _played[^1];
                    }
                }
            }

            return PlayersInOrder.FirstOrDefault();
        }

        public void NextPlayer()
        {
            var currentPlayer = CurrentPlayer;

            _played.Add(currentPlayer);

            if (NumberOfPlayersThatMayGetTurn == 0)
            {
                _played.Clear();
            }

            if (_game.Version <= 117)
            {
                CurrentPlayer = DetermineCurrentPlayer();
            }
        }

        public bool HasPlayersWithRoomForCardsBeforeWhite
        {
            get
            {
                var playerAfter = PlayerAfter(CurrentPlayer, true);

                while (playerAfter.Faction != Faction.White)
                {
                    if (playerAfter.HasRoomForCards)
                    {
                        return true;
                    }

                    playerAfter = PlayerAfter(playerAfter, true);
                }

                return false;
            }
        }

        public void NextRound()
        {
            FirstPlayer = PlayerAfter(FirstPlayer);

            _played.Clear();

            if (_game.Version <= 117)
            {
                CurrentPlayer = DetermineCurrentPlayer();
            }

            _round++;
        }

        public IEnumerable<Player> PlayersInOrder
        {
            get
            {
                var result = new List<Player>();
                var playerWithJuice = _game.CurrentJuice != null && (_game.CurrentJuice.Type == JuiceType.GoFirst || _game.CurrentJuice.Type == JuiceType.GoLast) ? _game.CurrentJuice.Player : null;

                if (playerWithJuice == null || NumberOfPlayersThatMayGetTurn > 1)
                {
                    var toAdd = FirstPlayer;
                    while (result.Count == 0 || toAdd != result[0])
                    {
                        if (toAdd != playerWithJuice)
                        {
                            result.Add(toAdd);
                        }

                        toAdd = PlayerAfter(toAdd, true);
                    }
                }

                if (playerWithJuice != null)
                {
                    if (_game.CurrentJuice.Type == JuiceType.GoFirst)
                    {
                        result.Insert(0, playerWithJuice);
                    }
                    else if (_game.CurrentJuice.Type == JuiceType.GoLast)
                    {
                        result.Add(playerWithJuice);
                    }
                }

                return result;
            }
        }

        public IEnumerable<SequenceElement> GetPlayersInSequence()
        {
            return PlayersInOrder.Select(p => new SequenceElement() { Player = p, HasTurn = CurrentPlayer == p });
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

        public static bool IsAfter(Game g, Player a, Player b)
        {
            var firstPlayer = DetermineFirstPlayer(g);

            for (int i = 0; i < g.MaximumNumberOfPlayers; i++)
            {
                int positionToCheck = (firstPlayer.PositionAtTable + i) % g.MaximumNumberOfPlayers;

                if (positionToCheck == b.PositionAtTable)
                {
                    return true;
                }
                else if (positionToCheck == a.PositionAtTable)
                {
                    return false;
                }
            }

            return false;
        }
    }

    public class SequenceElement
    {
        public Player Player;
        public bool HasTurn;
    }
}

