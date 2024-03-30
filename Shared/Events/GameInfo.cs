/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;

namespace Treachery.Shared
{
    public class GameInfo
    {
        public int HostID;
        public bool HostParticipates;
        public string GameName;
        public bool HasPassword;
        public int ExpansionLevel;
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
        public DateTime? LastAction;

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
