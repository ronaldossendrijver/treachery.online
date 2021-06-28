/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;

namespace Treachery.Shared
{
    public class Leader : IHero
    {
        public Leader(int id)
        {
            Id = id;
        }

        public const int VARIABLEVALUE = 99;

        public Faction Faction { get; set; }

        public string Name
        {
            get
            {
                return Skin.Current.GetPersonName(this);
            }
        }

        public bool Is(Faction f)
        {
            return Faction == f;
        }

        public int Value { get; set; }

        public int ValueInCombatAgainst(IHero opposingHero)
        {
            if (Value == VARIABLEVALUE)
            {
                if (opposingHero == null || opposingHero.Value == Leader.VARIABLEVALUE)
                {
                    return 0;
                }
                else
                {
                    return opposingHero.Value;
                }
            }
            else
            {
                return Value;
            }
        }

        public int CostToRevive
        {
            get
            {
                if (Value == VARIABLEVALUE)
                {
                    return 6;
                }
                else
                {
                    return Value;
                }
            }
        }

        public int Id { get; private set; }

        public int SkinId
        {
            get
            {
                return Id;
            }
        }

        public override bool Equals(object obj)
        {
            return (obj is Leader l && l.Id == Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

        public bool IsTraitor(IHero hero)
        {
            return hero == this;
        }

        public bool IsFaceDancer(IHero hero)
        {
            return hero == this;
        }
    }

    public class LeaderState : ICloneable
    {
        private static int moment;

        public Territory CurrentTerritory { get; set; }

        //even = alive, odd = dead
        public int DeathCounter { get; set; }

        public int TimeOfDeath { get; set; }

        public bool Alive
        {
            get
            {
                return (DeathCounter % 2) == 0;
            }
        }

        public bool IsFaceDownDead
        {
            get
            {
                return (DeathCounter % 4) == 3;
            }
        }

        public void Kill()
        {
            DeathCounter++;
            TimeOfDeath = moment++;
        }

        public void Assassinate()
        {
            Kill();
            if (!IsFaceDownDead)
            {
                Revive();
                Kill();
            }
        }

        public void Revive()
        {
            DeathCounter++;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}