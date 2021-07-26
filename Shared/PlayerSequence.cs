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
        public IList<Player> Players { get; set; }

        public int RoundStartedAt { get; set; }

        private int MaximumNumberOfPlayers { get; set; }

        public int Current { get; set; }

        private int _direction = 1;

        public PlayerSequence(IEnumerable<Player> players, int maximumNumberOfPlayers)
        {
            Players = players.ToList();
            MaximumNumberOfPlayers = maximumNumberOfPlayers;
        }

        public void Start(Player p, int direction)
        {
            _direction = direction;
            Current = p.PositionAtTable;
            RoundStartedAt = Current;
        }

        public void Start(Game game, bool ignorePlayersThatCantBid, int direction)
        {
            _direction = direction;
            var startLookingInSector = (int)Math.Ceiling((float)game.SectorInStorm * game.MaximumNumberOfPlayers / Map.NUMBER_OF_SECTORS) % game.MaximumNumberOfPlayers;
            Current = FindNearestPlayerPosition(startLookingInSector, ignorePlayersThatCantBid);
            RoundStartedAt = Current;
        }
             
        public Player CurrentPlayer => Players.Where(p => p.PositionAtTable == Current).SingleOrDefault();

        public Faction CurrentFaction => CurrentPlayer != null ? CurrentPlayer.Faction : Faction.None;

        public void NextRound(bool ignorePlayersThatCantBid)
        {
            Current = FindNearestPlayerPosition(RoundStartedAt + _direction, ignorePlayersThatCantBid);
            RoundStartedAt = Current;
        }

        public void NextPlayer(bool ignorePlayersThatCantBid)
        {
            Current = FindNearestPlayerPosition(Current + _direction, ignorePlayersThatCantBid);
        }

        //Returns a position number at the table occupied by a player nearest to the indicated position. The number of positions is zero based and depends on the Maximum number of players selected at game start.
        private int FindNearestPlayerPosition(int positionToStartLooking, bool ignorePlayersThatCantBid)
        {
            int position = (MaximumNumberOfPlayers + positionToStartLooking) % MaximumNumberOfPlayers;
            for (int i = 0; i < MaximumNumberOfPlayers; i++)
            {
                if (Players.Any(p => p.PositionAtTable == (position % MaximumNumberOfPlayers) && (!ignorePlayersThatCantBid || p.HasRoomForCards)))
                {
                    return position;
                }
                else
                {
                    position = (MaximumNumberOfPlayers + position + _direction) % MaximumNumberOfPlayers;
                }
            }

            return -1;
        }

        public override string ToString()
        {
            return string.Format(string.Join("->", Players.OrderBy(p => p.PositionAtTable).Select(p => string.Format("{0} ({1})", p.Name, p.PositionAtTable))) + ", Current: {0}", Current);
        }

        public IEnumerable<SequenceElement> GetPlayersInSequence(Game g)
        {
            var result = new List<SequenceElement>();
            for (int i = 0; i < g.MaximumNumberOfPlayers; i++)
            {
                int pos = (RoundStartedAt + i) % g.MaximumNumberOfPlayers;
                var playerAtPosition = Players.FirstOrDefault(p => p.PositionAtTable == (RoundStartedAt + _direction * i) % g.MaximumNumberOfPlayers);
                if (playerAtPosition != null)
                {
                    var elt = new SequenceElement() { Player = playerAtPosition, HasTurn = pos == Current };
                    result.Add(elt);
                }
            }
            return result;
        }
    }

    public class SequenceElement
    {
        public Player Player;
        public bool HasTurn;
    }
}
