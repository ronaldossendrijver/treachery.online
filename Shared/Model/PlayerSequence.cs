/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;

namespace Treachery.Shared;

public class PlayerSequence
{
    private List<Player> Players { get; }
    private readonly List<Player> _played = new();
    private readonly Game _game;
    private readonly bool _skipPlayersThatCantBidOnCards;
    private readonly int _direction;
    private int _round;
    private int _fullyCircled;

    public PlayerSequence(Game game, bool skipPlayersThatCantBidOnCards, int direction, Player toStartWith, bool beginNextToThatPlayer)
    {
        _game = game;
        _skipPlayersThatCantBidOnCards = skipPlayersThatCantBidOnCards;
        _direction = direction;
        _round = 1;
        _fullyCircled = 0;

        Players = _game.Players.OrderBy(p => _direction * p.Seat).ToList();
        FirstPlayer = toStartWith;

        if (beginNextToThatPlayer && NumberOfPlayersThatMayGetTurn > 1) FirstPlayer = PlayerAfter(FirstPlayer);

        ChangeFirstPlayerIfTheyCantBid();

        if (_game.Version <= 117) CurrentPlayer = DetermineCurrentPlayer();
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
            while (!MayGetTurn(FirstPlayer) || JuiceForcesLast(FirstPlayer)) FirstPlayer = PlayerAfter(FirstPlayer);
    }

    private int NumberOfPlayersThatMayGetTurn => Players.Count(p => !_skipPlayersThatCantBidOnCards || p.HasRoomForCards);

    private bool MayGetTurn(Player p)
    {
        return !_skipPlayersThatCantBidOnCards || p.HasRoomForCards;
    }

    private bool JuiceForcesLast(Player p)
    {
        return _game.CurrentJuice != null && _game.CurrentJuice.Type == JuiceType.GoLast &&
               _game.CurrentJuice.Player == p;
    }

    public void CheckCurrentPlayer()
    {
        if (_round == 1 && !_played.Any()) ChangeFirstPlayerIfTheyCantBid();

        CurrentPlayer = DetermineCurrentPlayer();
    }

    private Player PlayerAfter(Player currentPlayer, bool includePlayersThatCantBid = false)
    {
        var currentPlayerFound = false;

        Player firstFound = null;
        foreach (var p in Players)
        {
            if (currentPlayerFound)
            {
                if (includePlayersThatCantBid || MayGetTurn(p)) return p;
            }
            else if (p == currentPlayer)
            {
                currentPlayerFound = true;
            }

            if (firstFound == null && (includePlayersThatCantBid || MayGetTurn(p))) firstFound = p;
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
                return DetermineCurrentPlayer();
            return _currentPlayer;
        }

        private set => _currentPlayer = value;
    }

    private Player DetermineCurrentPlayer()
    {
        if (_played.Count > 0)
        {
            var found = false;
            foreach (var p in PlayersInOrder)
                if (found)
                    return p;
                else
                    found = p == _played[^1];
        }

        return PlayersInOrder.FirstOrDefault();
    }

    public void NextPlayer()
    {
        var currentPlayer = CurrentPlayer;

        _played.Add(currentPlayer);

        if (NumberOfPlayersThatMayGetTurn == 0) _played.Clear();

        if (_game.Version <= 117) CurrentPlayer = DetermineCurrentPlayer();

        if (CurrentPlayer == FirstPlayer) _fullyCircled++;
    }

    public bool HasPassedWhite
    {
        get
        {
            if (_fullyCircled > 0) return true;

            var current = CurrentPlayer;

            foreach (var p in PlayersInOrder)
                if (p == current)
                    return false;
                else if (p.Faction == Faction.White) return true;

            return false;
        }
    }

    public void NextRound()
    {
        FirstPlayer = PlayerAfter(FirstPlayer);

        _played.Clear();

        if (_game.Version <= 117) CurrentPlayer = DetermineCurrentPlayer();

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
                    if (toAdd != playerWithJuice) result.Add(toAdd);

                    toAdd = PlayerAfter(toAdd, true);
                }
            }

            if (playerWithJuice != null)
            {
                if (_game.CurrentJuice.Type == JuiceType.GoFirst)
                    result.Insert(0, playerWithJuice);
                else if (_game.CurrentJuice.Type == JuiceType.GoLast) result.Add(playerWithJuice);
            }

            return result;
        }
    }

    public IEnumerable<SequenceElement> GetPlayersInSequence()
    {
        return PlayersInOrder.Select(p => new SequenceElement { Player = p, HasTurn = CurrentPlayer == p });
    }

    public static Player DetermineFirstPlayer(Game g)
    {
        var startLookingInSector = (int)Math.Ceiling((float)g.SectorInStorm * g.MaximumPlayers / Map.NUMBER_OF_SECTORS) % g.MaximumPlayers;

        Player result = null;

        var position = (g.MaximumPlayers + startLookingInSector) % g.MaximumPlayers;
        for (var i = 0; i < g.MaximumPlayers; i++)
        {
            result = g.Players.FirstOrDefault(p => p.Seat == position);
            if (result != null)
                return result;
            position = Mod(g.MaximumPlayers + position + 1, g.MaximumPlayers);
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

        for (var i = 0; i < g.MaximumPlayers; i++)
        {
            var positionToCheck = (firstPlayer.Seat + i) % g.MaximumPlayers;

            if (positionToCheck == b.Seat)
                return true;
            if (positionToCheck == a.Seat) return false;
        }

        return false;
    }
}

public class SequenceElement
{
    public Player Player;
    public bool HasTurn;
}