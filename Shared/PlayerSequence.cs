/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class PlayerSequence
    {
        private readonly List<Player> _played = new List<Player>();
        private readonly Game _game;
        private readonly bool _skipPlayersThatCantBidOnCards;
        private readonly int _direction;
        private Player _first;

        public PlayerSequence(Game game, bool skipPlayersThatCantBidOnCards, int direction, Player toStartWith, bool beginNextToThatPlayer)
        {
            //Console.WriteLine("Start PlayerSequence");

            _game = game;
            _skipPlayersThatCantBidOnCards = skipPlayersThatCantBidOnCards;
            _direction = direction;
            _first = toStartWith;

            if (beginNextToThatPlayer && PlayersInOrderThatMayGetTurn.Count() > 1)
            {
                _first = PlayerAfter(_first);
            }

            if (_game.Version <= 117)
            {
                while (!MayGetTurn(_first) || JuiceForcesLast(_first) && PlayersInOrderThatMayGetTurn.Count() > 1)
                {
                    _first = PlayerAfter(_first);
                }

                CurrentPlayer = DetermineCurrentPlayer();
            }


            //Console.Write("Player sequence started: ");
            //foreach (var player in PlayersInOrderThatMayGetTurn)
            //{
            //Console.Write(" -> " + player.Faction + " (" + player.PositionAtTable + ")");
            //}

            //////Console.WriteLine(". Storm at: " + _game.SectorInStorm + ", first player: " + _first + " (position: " + _first.PositionAtTable + "), current player: " + CurrentPlayer);
            //Console.WriteLine("End PlayerSequence");
        }

        public PlayerSequence(Game game, bool skipPlayersThatCantBidOnCards, int direction)
            : this(game, skipPlayersThatCantBidOnCards, direction, DetermineFirstPlayer(game), false)
        {

        }

        public PlayerSequence(Game game, bool skipPlayersThatCantBidOnCards)
            : this(game, skipPlayersThatCantBidOnCards, 1, DetermineFirstPlayer(game), false)
        {

        }

        public PlayerSequence(Game game)
            : this(game, false, 1, DetermineFirstPlayer(game), false)
        {

        }

        private IEnumerable<Player> PlayersInOrder => _game.Players
            .OrderBy(p => _direction * p.PositionAtTable);

        private IEnumerable<Player> PlayersInOrderThatMayGetTurn => _game.Players
            .Where(p => !_skipPlayersThatCantBidOnCards || p.HasRoomForCards)
            .OrderBy(p => _direction * p.PositionAtTable);

        private bool MayGetTurn(Player p)
        {
            return !_skipPlayersThatCantBidOnCards || p.HasRoomForCards;
        }

        private bool JuiceForcesLast(Player p)
        {
            return _game.CurrentJuice != null && _game.CurrentJuice.Type == JuiceType.GoLast && _game.CurrentJuice.Player == p;
        }

        public void CheckCurrentPlayer()
        {
            //Console.WriteLine("Start CheckCurrentPlayer");

            if (!_played.Any())
            {
                while (!MayGetTurn(_first) || JuiceForcesLast(_first) && PlayersInOrderThatMayGetTurn.Count() > 1)
                {
                    _first = PlayerAfter(_first);
                }
            }

            CurrentPlayer = DetermineCurrentPlayer();

            //Console.WriteLine("End CheckCurrentPlayer");
        }

        private Player PlayerAfter(Player currentPlayer, bool includePlayersThatCantBid = false)
        {
            //Console.WriteLine("Start PlayerAfter");
            bool currentPlayerFound = false;

            Player first = null;
            foreach (Player p in PlayersInOrder)
            {
                if (first == null && (includePlayersThatCantBid || MayGetTurn(p))) first = p;

                if (currentPlayerFound)
                {
                    if (includePlayersThatCantBid || MayGetTurn(p))
                    {
                        ////////Console.WriteLine("PlayerAfter1 " + currentPlayer + " -> " + p);
                        //Console.WriteLine("PlayerAfter: currentPlayerFound -> " + p);
                        return p;
                    }
                }
                else if (p == currentPlayer)
                {
                    currentPlayerFound = true;
                }
            }

            //Console.WriteLine("PlayerAfter: firstPlayer -> " + first);

            if (first == null)
            {
                //Console.WriteLine("PlayerAfter: first is null!!! PlayerInOrder: " + Skin.Current.Join(PlayersInOrder) + ", MayGetTurn: " + Skin.Current.Join(PlayersInOrder.Select(p => MayGetTurn(p))));
            }

            //Skip players that can't bid
            /*if (!includePlayersThatCantBid)
            {
                while (!MayGetTurn(first))
                {
                    first = PlayerAfter(first);
                }
            }*/

            ////////Console.WriteLine("PlayerAfter2 " + currentPlayer + " -> " + first);
            //Console.WriteLine("End PlayerAfter");
            return first;
        }   

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
            //Console.WriteLine("Start DetermineCurrentPlayer");
            /*
            //Console.WriteLine("JuiceForcesFirstPlayer: {0}, _played.Count: {1}, JuiceForcesLastPlayer: {2}, PlayersInOrderThatMayGetTurn.Count(p => !_played.Contains(p)): {3}, _first: {4}",
                _game.JuiceForcesFirstPlayer,
                _played.Count,
                _game.JuiceForcesLastPlayer,
                PlayersInOrderThatMayGetTurn.Count(p => !_played.Contains(p)),
                _first);*/

            if (_game.JuiceForcesFirstPlayer && MayGetTurn(_game.CurrentJuice?.Player) && _played.Count == 0)
            {
                //Console.WriteLine("End DetermineCurrentPlayer");
                return _game.CurrentJuice.Player;
            }
            else if (_game.JuiceForcesLastPlayer && MayGetTurn(_game.CurrentJuice?.Player) && PlayersInOrderThatMayGetTurn.Count(p => !_played.Contains(p)) == 1)
            {
                //Console.WriteLine("End DetermineCurrentPlayer");
                return _game.CurrentJuice.Player;
            }
            else if (_played.Count == 0)
            {
                if (_skipPlayersThatCantBidOnCards && !_first.HasRoomForCards || _first == _game.CurrentJuice?.Player)
                {
                    //Console.WriteLine("End DetermineCurrentPlayer");
                    return PlayerAfter(_first);
                }
                else
                {
                    //Console.WriteLine("End DetermineCurrentPlayer");
                    return _first;
                }
            }
            else
            {
                ////////Console.WriteLine("Continuing with " + PlayerAfter(_played[_played.Count - 1]) + " who is after " + _played[_played.Count - 1]);
                //Console.WriteLine("End DetermineCurrentPlayer");
                return PlayerAfter(_played[_played.Count - 1]);
            }
        }

        public void NextPlayer()
        {
            //////Console.WriteLine("=== START == NextPlayer == Currentplayer: {0} ===", CurrentPlayer);

            var currentPlayer = CurrentPlayer;

            _played.Add(currentPlayer);

            //////Console.WriteLine("Played: " + Skin.Current.Join(_played));

            if (PlayerAfter(currentPlayer) == _first && !_game.JuiceForcesLastPlayer)
            {
                //////Console.WriteLine("CurrentPlayer: {0}, PlayerAfter: {1}, _first: {2} -> Starting new...", currentPlayer, PlayerAfter(currentPlayer), _first);
                _played.Clear();
            }

            if (_game.Version <= 117)
            {
                CurrentPlayer = DetermineCurrentPlayer();
            }

            //////Console.WriteLine("=== END == NextPlayer == Currentplayer: {0} ===", CurrentPlayer);
        }

        public void NextRound()
        {
            if (_played.Any())
            {
                _first = PlayerAfter(_played[0]);
            }
            else
            {
                _first = PlayerAfter(CurrentPlayer);
            }

            _played.Clear();

            if (_game.Version <= 117)
            {
                CurrentPlayer = DetermineCurrentPlayer();
            }
        }

        public IEnumerable<SequenceElement> GetPlayersInSequence()
        {
            var result = new List<SequenceElement>();

            if (_game.JuiceForcesFirstPlayer)
            {
                result.Add(new SequenceElement() { Player = _game.CurrentJuice.Player, HasTurn = CurrentPlayer == _game.CurrentJuice.Player });
            }

            //result.AddRange(_played.Select(p => new SequenceElement() { Player = p, HasTurn = CurrentPlayer == p }));

            var toAdd = _first;
            while (!result.Any(se => se.Player == toAdd))
            {
                if (toAdd != _game.CurrentJuice?.Player)
                {
                    result.Add(new SequenceElement() { Player = toAdd, HasTurn = CurrentPlayer == toAdd });
                }

                toAdd = PlayerAfter(toAdd, true);
            }

            if (_game.JuiceForcesLastPlayer)
            {
                result.Add(new SequenceElement() { Player = _game.CurrentJuice.Player, HasTurn = CurrentPlayer == _game.CurrentJuice.Player });
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

    public class SequenceElement
    {
        public Player Player;
        public bool HasTurn;
    }
}

