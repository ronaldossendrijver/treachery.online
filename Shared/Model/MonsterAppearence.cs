/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class MonsterAppearence
    {
        public Territory Territory;
        public bool IsGreatMonster;

        public MonsterAppearence(Territory territory, bool isGreatMonster)
        {
            Territory = territory;
            IsGreatMonster = isGreatMonster;
        }

        public Concept DescribingConcept => IsGreatMonster ? Concept.GreatMonster : Concept.Monster;
    }
}
