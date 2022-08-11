/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;

namespace Treachery.Shared
{
    public class GameInfo
    {
        public int HostID;
        public bool HostParticipates;
        public string GameName;
        public bool HasPassword;
        public Phase CurrentPhase;
        public MainPhase CurrentMainPhase;
        public int CurrentTurn;
        public int MaximumNumberOfPlayers;
        public int MaximumNumberOfTurns;
        public List<Faction> FactionsInPlay;
        public string[] Players;
        public int NumberOfBots;
        public List<Rule> Rules;
        public bool InviteOthers;

        public override bool Equals(object obj)
        {
            return obj is GameInfo info && info.HostID == HostID;
        }

        public override int GetHashCode()
        {
            return HostID.GetHashCode();
        }
    }
}
