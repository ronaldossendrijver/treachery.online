/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

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

        public override bool Equals(object obj) => (obj is Leader l && l.Id == Id);

        public override int GetHashCode() => Id.GetHashCode();

        public bool IsTraitor(IHero hero) => hero == this;

        public bool IsFaceDancer(IHero hero) => hero == this;
    }
}