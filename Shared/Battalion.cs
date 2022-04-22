/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;

namespace Treachery.Shared
{
    public class Battalion : ICloneable
    {
        public Faction Faction { get; set; }
        public int AmountOfForces { get; set; }
        public int AmountOfSpecialForces { get; set; }

        public bool Is(Faction f)
        {
            return Faction == f;
        }

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

        public Battalion TakeHalf()
        {
            return new Battalion() { Faction = Faction, AmountOfForces = (int)Math.Ceiling(0.5 * AmountOfForces), AmountOfSpecialForces = (int)Math.Ceiling(0.5 * AmountOfSpecialForces) };
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
                    return new Battalion() { Faction = Faction, AmountOfForces = normalAmountToTake, AmountOfSpecialForces = specialAmountToTake };
                }
                else
                {
                    int toTake = amount;
                    int normalAmountToTake = Math.Min(AmountOfForces, toTake);
                    toTake -= normalAmountToTake;
                    int specialAmountToTake = Math.Min(AmountOfSpecialForces, toTake);
                    return new Battalion() { Faction = Faction, AmountOfForces = normalAmountToTake, AmountOfSpecialForces = specialAmountToTake };
                }
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
