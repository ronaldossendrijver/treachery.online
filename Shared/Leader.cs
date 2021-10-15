/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;

namespace Treachery.Shared
{
    public class Leader : IHero
    {
        public int Id { get; private set; }


        public const int VARIABLEVALUE = 99;

        public Faction Faction { get; set; }

        public int Value { get; set; }

        public Leader(int id)
        {
            Id = id;
        }

        public string Name => Skin.Current.GetPersonName(this);
        
        public HeroType HeroType { get; set; }

        public bool Is(Faction f) => Faction == f;

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

        public int SkinId => Id;

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
}