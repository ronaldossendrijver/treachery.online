/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
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

        public IEnumerable<Location> LocationsWithForcesThatCanRide(Game g)
        {
            var p = g.GetPlayer(Faction.Yellow);

            if (p == null || IsGreatMonster)
            {
                return Array.Empty<Location>();
            }
            else if (g.Version < 136)
            {
                return Territory.Locations.Where(l => p.AnyForcesIn(l) > 0);
            }
            else
            {
                bool mayRideFromStorm = p.Is(Faction.Yellow) && g.Applicable(Rule.YellowMayMoveIntoStorm);
                return Territory.Locations.Where(l => (mayRideFromStorm || !g.IsInStorm(l)) && p.AnyForcesIn(l) > 0);
            }
        }
    }
}
