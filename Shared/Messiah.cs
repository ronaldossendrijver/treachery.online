/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class Messiah : IHero
    {
        public Messiah()
        {
        }

        public int Value => 2;

        public int ValueInCombatAgainst(IHero opposingHero) => Value;

        public string Name => Skin.Current.Describe(Concept.Messiah);

        public Faction Faction => Faction.Green;

        public HeroType HeroType => HeroType.Messiah;

        public bool Is(Faction f) => Faction == f;

        public int CostToRevive => Value;

        public int Id { get; set; }

        public int SkinId { get; set; }

        public override string ToString()
        {
            return Skin.Current.Describe(Concept.Messiah);
        }

        public bool IsTraitor(IHero hero) => false;

        public bool IsFaceDancer(IHero hero) => false;
    }
}