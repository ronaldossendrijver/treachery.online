/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

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

        public bool HasForcesThatCanRide(Game g, Player p)
        {
            if (g.Version < 136)
            {
                return p.AnyForcesIn(Territory) > 0;
            }
            else
            {
                bool mayRideFromStorm = p.Is(Faction.Yellow) && g.Applicable(Rule.YellowMayMoveIntoStorm);
                return Territory.Locations.Any(l => (mayRideFromStorm || !g.IsInStorm(l)) && p.AnyForcesIn(l) > 0);
            }
        }
    }
}
