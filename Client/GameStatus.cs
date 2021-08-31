/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class GameStatus
    {
        public string DescriptionWhenAwaited { get; set; }

        public string DescriptionWhenWaiting { get; set; }

        public IEnumerable<SequenceElement> WaitingInSequence { get; set; } = Array.Empty<SequenceElement>();

        public IEnumerable<Faction> WaitingForFactions { get; set; } = Array.Empty<Faction>();

        public bool WaitingForHost { get; set; } = false;

        public GameStatus(Player player, bool isHost, string descriptionWhenAwaited, string descriptionWhenWaiting, IEnumerable<SequenceElement> waitingInSequence)
        {
            DescriptionWhenAwaited = descriptionWhenAwaited;
            DescriptionWhenWaiting = descriptionWhenWaiting;
            WaitingInSequence = waitingInSequence;
        }

        public GameStatus(Player player, bool isHost, string descriptionWhenAwaited, string descriptionWhenWaiting, Faction waitingForFaction) :
            this(player, isHost, descriptionWhenAwaited, descriptionWhenWaiting, new Faction[] { waitingForFaction })
        {
        }

        public GameStatus(Player player, bool isHost, string descriptionWhenAwaited, string descriptionWhenWaiting, Player waitingForPlayer) :
            this(player, isHost, descriptionWhenAwaited, descriptionWhenWaiting, new Faction[] { waitingForPlayer.Faction })
        {
        }

        public GameStatus(Player player, bool isHost, string descriptionWhenAwaited, string descriptionWhenWaiting, IEnumerable<Faction> waitingForFactions)
        {
            DescriptionWhenAwaited = descriptionWhenAwaited;
            DescriptionWhenWaiting = descriptionWhenWaiting;
            WaitingForFactions = waitingForFactions;
        }

        public GameStatus(Player player, bool isHost, string descriptionWhenAwaited, string descriptionWhenWaiting, IEnumerable<Player> waitingForPlayers) :
            this(player, isHost, descriptionWhenAwaited, descriptionWhenWaiting, waitingForPlayers.Select(p => p.Faction))
        {
        }

        public GameStatus(Player player, bool isHost, string descriptionWhenAwaited, string descriptionWhenWaiting)
        {
            DescriptionWhenAwaited = descriptionWhenAwaited;
            DescriptionWhenWaiting = descriptionWhenWaiting;
            WaitingForHost = true;
        }

        public GameStatus(Player player, bool isHost, string description)
        {
            DescriptionWhenAwaited = description;
            DescriptionWhenWaiting = description;
            WaitingForHost = true;
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
