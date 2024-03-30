/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

namespace Treachery.Shared
{
    public class ResourceCard
    {
        public bool IsSandTrout { get; set; } = false;
        public bool IsGreatMaker { get; set; } = false;

        public int SkinId { get; private set; }

        public Location Location { get; set; } = null;

        public Location DiscoveryLocation { get; set; } = null;

        public ResourceCard(int skinId)
        {
            SkinId = skinId;
        }

        public bool IsShaiHulud => Location == null && !IsSandTrout && !IsGreatMaker;

        public bool IsSpiceBlow => Location != null;

        public bool IsDiscovery => DiscoveryLocation != null;

        public Territory Territory => Location?.Territory;

        public override string ToString()
        {
            if (Message.DefaultDescriber != null)
            {
                return Message.DefaultDescriber.Describe(this) + "*";
            }
            else
            {
                return base.ToString();
            }
        }
    }
}