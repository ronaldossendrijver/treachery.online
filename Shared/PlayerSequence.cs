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

        public int Current { get; set; }

        public PlayerSequence()
        {

        }

        public PlayerSequence(IEnumerable<Player> players)
        {
            Players = players.ToList();
        }

        public void Start(Game game, bool ignorePlayersThatCantBid)
        {
            var startLooking = (int)Math.Ceiling((float)game.SectorInStorm * game.MaximumNumberOfPlayers / Map.NUMBER_OF_SECTORS) % game.MaximumNumberOfPlayers;
            Current = FindNearestPlayerPosition(game, startLooking, ignorePlayersThatCantBid);
            RoundStartedAt = Current;
        }

        public Player CurrentPlayer
        {
            get
            {
                return Players.Where(p => p.PositionAtTable == Current).Single();
            }
        }

        public Player RoundStartedAtPlayer
        {
            get
            {
                return Players.Where(p => p.PositionAtTable == RoundStartedAt).Single();
            }
        }

        public Faction CurrentFaction
        {
            get
            {
                return CurrentPlayer.Faction;
            }
        }

        public void NextRound(Game game, bool ignorePlayersThatCantBid)
        {
            Current = FindNearestPlayerPosition(game, RoundStartedAt + 1, ignorePlayersThatCantBid);
            RoundStartedAt = Current;
        }

        public void NextPlayer(Game game, bool ignorePlayersThatCantBid)
        {
            Current = FindNearestPlayerPosition(game, Current + 1, ignorePlayersThatCantBid);
        }

        private int FindNearestPlayerPosition(Game game, int positionToStartLooking, bool ignorePlayersThatCantBid)
        {
            int position = positionToStartLooking % game.MaximumNumberOfPlayers;
            for (int i = 0; i < game.MaximumNumberOfPlayers; i++)
            {
                if (Players.Any(p => p.PositionAtTable == position && (!ignorePlayersThatCantBid || p.MayBidOnCards)))
                {
                    return position;
                }
                else
                {
                    position = (position + 1) % game.MaximumNumberOfPlayers;
                }
            }

            return -1;
        }

        public override string ToString()
        {
            return string.Format(string.Join("->", Players.OrderBy(p => p.PositionAtTable).Select(p => string.Format("{0} ({1})", p.Name, p.PositionAtTable))) + ", Current: {0}", Current);
        }

        public IEnumerable<SequenceElement> GetFactionsInSequence(Game g)
        {
            var result = new List<SequenceElement>();
            for (int i = 0; i < g.MaximumNumberOfPlayers; i++)
            {
                int pos = (RoundStartedAt + i) % g.MaximumNumberOfPlayers;
                var playerAtPosition = Players.FirstOrDefault(p => p.PositionAtTable == (RoundStartedAt + i) % g.MaximumNumberOfPlayers);
                if (playerAtPosition != null)
                {
                    var elt = new SequenceElement() { Faction = playerAtPosition.Faction, HasTurn = pos == Current };
                    result.Add(elt);
                }
            }
            return result;
        }
    }

    public class SequenceElement
    {
        public Faction Faction;
        public bool HasTurn;
    }
}
