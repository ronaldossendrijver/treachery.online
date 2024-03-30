/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using System;

namespace Treachery.Shared
{
    public class Battalion : ICloneable
    {
        public Faction Faction { get; set; }

        public Location Location { get; set; }

        public int AmountOfForces { get; set; }

        public int AmountOfSpecialForces { get; set; }

        public Battalion(Faction faction, int amountOfForces, int amountOfSpecialForces, Location location)
        {
            Faction = faction;
            AmountOfForces = amountOfForces;
            AmountOfSpecialForces = amountOfSpecialForces;
            Location = location;
        }

        public bool Is(Faction f) => Faction == f;

        public bool CanOccupy => AmountOfForces > 0 || (Faction != Faction.Blue && AmountOfSpecialForces > 0);

        public int TotalAmountOfForces => AmountOfForces + AmountOfSpecialForces;

        public void ChangeForces(int amount)
        {
            if (AmountOfForces + amount >= 0)
            {
                AmountOfForces += amount;
            }
        }

        public void ChangeSpecialForces(int amount)
        {
            if (AmountOfSpecialForces + amount >= 0)
            {
                AmountOfSpecialForces += amount;
            }
        }

        public void Clear()
        {
            AmountOfForces = 0;
            AmountOfSpecialForces = 0;
        }

        public Battalion TakeHalf()
        {
            return new Battalion(Faction, (int)Math.Ceiling(0.5 * AmountOfForces), (int)Math.Ceiling(0.5 * AmountOfSpecialForces), Location);
        }

        public Battalion Take(int amount, bool preferSpecial)
        {
            if (amount >= TotalAmountOfForces)
            {
                return this;
            }
            else
            {
                if (preferSpecial)
                {
                    int toTake = amount;
                    int specialAmountToTake = Math.Min(AmountOfSpecialForces, toTake);
                    toTake -= specialAmountToTake;
                    int normalAmountToTake = Math.Min(AmountOfForces, toTake);
                    return new Battalion(Faction, normalAmountToTake, specialAmountToTake, Location);
                }
                else
                {
                    int toTake = amount;
                    int normalAmountToTake = Math.Min(AmountOfForces, toTake);
                    toTake -= normalAmountToTake;
                    int specialAmountToTake = Math.Min(AmountOfSpecialForces, toTake);
                    return new Battalion(Faction, normalAmountToTake, specialAmountToTake, Location);
                }
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
