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

        public IEnumerable<Player> WaitingForPlayers { get; set; } = Array.Empty<Player>();

        public List<FlashInfo> FlashInfo = new List<FlashInfo>();

        public bool WaitingForHost { get; set; } = false;

        public int Timeout { get; set; } = 0;

        public GameStatus(string descriptionWhenAwaited, string descriptionWhenWaiting, Player waitingForPlayer) :
            this(descriptionWhenAwaited, descriptionWhenWaiting, new Player[] { waitingForPlayer })
        {
        }

        public GameStatus(string descriptionWhenAwaited, string descriptionWhenWaiting, IEnumerable<Player> waitingForPlayers)
        {
            DescriptionWhenAwaited = descriptionWhenAwaited;
            DescriptionWhenWaiting = descriptionWhenWaiting;
            WaitingForPlayers = waitingForPlayers;
        }

        public GameStatus(string descriptionWhenAwaited, string descriptionWhenWaiting)
        {
            DescriptionWhenAwaited = descriptionWhenAwaited;
            DescriptionWhenWaiting = descriptionWhenWaiting;
            WaitingForHost = true;
        }

        public GameStatus(string description)
        {
            DescriptionWhenAwaited = description;
            DescriptionWhenWaiting = description;
            WaitingForHost = true;
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
