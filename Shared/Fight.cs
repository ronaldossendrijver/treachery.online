/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class Fight
    {
        public Territory Territory { get; private set; }
        
        public Faction Faction { get; private set; }

        public Fight(Territory territory, Faction faction) { 
        
            Territory = territory;
            Faction = faction;
        }
    }
}
