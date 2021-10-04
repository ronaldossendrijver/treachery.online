/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Treachery.Shared;

namespace Treachery.Client
{
    public class GameStatus
    {
        public string DescriptionWhenAwaited { get; set; }

        public string DescriptionWhenWaiting { get; set; }

        public IEnumerable<SequenceElement> WaitingInSequence { get; set; } = Array.Empty<SequenceElement>();

        public IEnumerable<Faction> WaitingForFactions { get; set; } = Array.Empty<Faction>();

        public List<FlashInfo> FlashInfo = new List<FlashInfo>();

        public bool WaitingForHost { get; set; } = false;

        public int Timeout { get; set; } = 0;

        public GameStatus(string descriptionWhenAwaited, string descriptionWhenWaiting, Faction waitingForFaction) :
            this(descriptionWhenAwaited, descriptionWhenWaiting, new Faction[] { waitingForFaction })
        {
        }

        public GameStatus(string descriptionWhenAwaited, string descriptionWhenWaiting, Player waitingForPlayer) :
            this(descriptionWhenAwaited, descriptionWhenWaiting, new Faction[] { waitingForPlayer.Faction })
        {
        }

        public GameStatus(string descriptionWhenAwaited, string descriptionWhenWaiting, IEnumerable<Faction> waitingForFactions)
        {
            DescriptionWhenAwaited = descriptionWhenAwaited;
            DescriptionWhenWaiting = descriptionWhenWaiting;
            WaitingForFactions = waitingForFactions;
        }

        public GameStatus(string descriptionWhenAwaited, string descriptionWhenWaiting, IEnumerable<Player> waitingForPlayers) :
            this(descriptionWhenAwaited, descriptionWhenWaiting, waitingForPlayers.Select(p => p.Faction))
        {
        }

        public GameStatus(string descriptionWhenAwaited, string descriptionWhenWaiting, int timeout = 0)
        {
            DescriptionWhenAwaited = descriptionWhenAwaited;
            DescriptionWhenWaiting = descriptionWhenWaiting;
            WaitingForHost = true;
            Timeout = timeout;
        }

        public GameStatus(string description, int timeout = 0)
        {
            DescriptionWhenAwaited = description;
            DescriptionWhenWaiting = description;
            WaitingForHost = true;
            Timeout = timeout;
        }

        public bool WaitingForMe(Player player, bool isHost) => WaitingForHost && isHost ||
                WaitingForFactions.Contains(player.Faction) ||
                WaitingInSequence.Any(se => se.Player == player && se.HasTurn);

        public bool WaitingForOthers(Player player, bool isHost) => !WaitingForMe(player, isHost);

        public string GetDescription(Player player, bool isHost)
        {
            if (WaitingForMe(player, isHost))
            {
                return DescriptionWhenAwaited;
            }
            else
            {
                return DescriptionWhenWaiting;
            }
        }
    }
}
