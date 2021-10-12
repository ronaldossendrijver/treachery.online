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
        public string DescriptionWhenAwaited { get; private set; }

        public string DescriptionWhenWaiting { get; private set; }

        public IEnumerable<SequenceElement> WaitingInSequence { get; private set; } = Array.Empty<SequenceElement>();

        public IEnumerable<Player> WaitingForPlayers { get; private set; } = Array.Empty<Player>();

        public List<FlashInfo> FlashInfo { get; set; } = new List<FlashInfo>();

        public GameEvent TimedEvent { get; private set; } = null;

        public bool WaitingForHost { get; private set; } = false;

        public GameStatus(string descriptionWhenAwaited, string descriptionWhenWaiting, Player waitingForPlayer, GameEvent timedEvent = null) :
            this(descriptionWhenAwaited, descriptionWhenWaiting, new Player[] { waitingForPlayer }, timedEvent)
        {
        }

        public GameStatus(string descriptionWhenAwaited, string descriptionWhenWaiting, IEnumerable<Player> waitingForPlayers, GameEvent timedEvent = null)
        {
            DescriptionWhenAwaited = descriptionWhenAwaited;
            DescriptionWhenWaiting = descriptionWhenWaiting;
            WaitingForPlayers = waitingForPlayers;
            TimedEvent = timedEvent;
        }

        public GameStatus(string descriptionWhenAwaited, string descriptionWhenWaiting, GameEvent timedEvent = null)
        {
            DescriptionWhenAwaited = descriptionWhenAwaited;
            DescriptionWhenWaiting = descriptionWhenWaiting;
            WaitingForHost = true;
            TimedEvent = timedEvent;
        }

        public GameStatus(string description, GameEvent timedEvent = null) : this(description, description, timedEvent)
        {
        }

        public bool WaitingForMe(Player player, bool isHost) => WaitingForHost && isHost ||
                WaitingForPlayers.Contains(player) ||
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
