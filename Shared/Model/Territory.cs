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
using System.Linq;

namespace Treachery.Shared
{
    public class Territory : IIdentifiable, ICloneable
    {
        public Territory(int id)
        {
            Id = id;
        }

        public Territory(bool isHomeworld, bool isStronghold, bool hasReducedShippingCost, bool isDiscovery, bool isProtectedFromStorm, bool isProtectedFromWorm, int id) : this(id)
        {
            IsHomeworld = isHomeworld;
            IsStronghold = isStronghold;
            IsProtectedFromStorm = isProtectedFromStorm;
            IsProtectedFromWorm = isProtectedFromWorm;
            IsDiscovery = isDiscovery;
            HasReducedShippingCost = hasReducedShippingCost;
        }

        public int Id { get; private set; }

        public int SkinId => Id;

        public bool IsHomeworld { get; set; }

        public bool HasReducedShippingCost { get; set; }

        public bool IsDiscovery { get; set; }

        public bool IsDiscoveredStronghold => IsStronghold && IsDiscovery;

        public bool IsVisible => Locations.Any(l => l.Visible);

        public bool IsStronghold { get; set; }

        public bool IsProtectedFromStorm { get; set; }

        public bool IsProtectedFromWorm { get; set; }

        public StrongholdAdvantage Advantage { get; set; }

        public List<Location> Locations { get; private set; } = new List<Location>();

        public void AddLocation(Location l)
        {
            Locations.Add(l);
        }

        public Location MiddleLocation
        {
            get
            {
                var locations = Locations.ToList();
                return locations[(int)(0.5 * locations.Count)];
            }
        }

        public Location ResourceBlowLocation => Locations.FirstOrDefault(l => l.SpiceBlowAmount > 0);

        public Location DiscoveryTokenLocation => Locations.FirstOrDefault(l => l.DiscoveryTokenType != DiscoveryTokenType.None);

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

        public object Clone() => MemberwiseClone();
    }
}