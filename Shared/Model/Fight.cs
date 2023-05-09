/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class Fight : IIdentifiable
    {
        public Territory Territory { get; private set; }

        public Faction Faction { get; private set; }

        public int Id => Territory.Id + 1000 * (int)Faction;

        public Fight(Territory territory, Faction faction)
        {

            Territory = territory;
            Faction = faction;
        }

        
    }
}
