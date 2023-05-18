﻿/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class Leader : IHero
    {
        public int Id { get; private set; }

        public Faction Faction { get; set; }

        public int Value { get; set; }

        public Leader(int id)
        {
            Id = id;
        }

        public HeroType HeroType { get; set; }

        public bool Is(Faction f) => Faction == f;

        public int ValueInCombatAgainst(IHero opposingHero)
        {
            if (HeroType == HeroType.VariableValue)
            {
                if (opposingHero == null)
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
                if (HeroType == HeroType.VariableValue)
                {
                    return 6;
                }
                else if (HeroType == HeroType.Vidal)
                {
                    return 5;
                }
                else
                {
                    return Value;
                }
            }
        }

        public int SkinId => Id;

        public override bool Equals(object obj) => (obj is Leader l && l.Id == Id);

        public override int GetHashCode() => Id.GetHashCode();

        public bool IsTraitor(IHero hero) => hero == this;

        public bool IsFaceDancer(IHero hero) => hero == this;

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