/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class Messiah : IHero
    {
        public Messiah()
        {
        }

        public int Value
        {
            get
            {
                return 2;
            }
        }

        public int ValueInCombatAgainst(IHero opposingHero)
        {
            return Value;
        }

        public string Name
        {
            get
            {
                return Skin.Current.Describe(Concept.Messiah);
            }
        }

        public Faction Faction
        {
            get
            {
                return Faction.Green;
            }
        }

        public HeroType HeroType => HeroType.Messiah;

        public bool Is(Faction f)
        {
            return Faction == f;
        }

        public int CostToRevive
        {
            get
            {
                return Value;
            }
        }

        public int Id { get; set; }

        public int SkinId { get; set; }

        public override string ToString()
        {
            return Skin.Current.Describe(Concept.Messiah);
        }

        public bool IsTraitor(IHero hero)
        {
            return false;
        }

        public bool IsFaceDancer(IHero hero)
        {
            return false;
        }
    }
}