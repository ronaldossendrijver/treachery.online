/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public interface IHero : IIdentifiable
    {
        public int Value { get; }

        public int ValueInCombatAgainst(IHero opposingHero);

        public string Name { get; }

        public int SkinId { get; }

        public Faction Faction { get; }

        public int CostToRevive { get; }

        public bool IsTraitor(IHero hero);

        public bool IsFaceDancer(IHero hero);

        public bool Is(Faction f);

        public HeroType HeroType { get; }
    }
}
