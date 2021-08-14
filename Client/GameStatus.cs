/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;
using System;

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
            DetermineWaiting(player, isHost);
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
            DetermineWaiting(player, isHost);
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
            DetermineWaiting(player, isHost);
        }

        public GameStatus(Player player, bool isHost, string description)
        {
            DescriptionWhenAwaited = description;
            DescriptionWhenWaiting = description;
            WaitingForHost = true;
            DetermineWaiting(player, isHost);
        }

        public void DetermineWaiting(Player player, bool isHost)
        {
            WaitingForMe = WaitingForHost && isHost || WaitingForFactions.Contains(player.Faction) || WaitingInSequence.Any(se => se.Player == player && se.HasTurn);
        }

        public bool WaitingForMe { get; private set; }

        public bool WaitingForOthers => !WaitingForMe;

        public string Description
        {
            get
            {
                if (WaitingForMe) return DescriptionWhenAwaited;
                else return DescriptionWhenWaiting;
            }
        }
    }
}
