/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class AmbassadorPlaced : GameEvent
    {
        public AmbassadorPlaced(Game game) : base(game)
        {
        }

        public AmbassadorPlaced()
        {
        }

        public bool Passed;

        public Faction Faction;

        public int _strongholdId;

        [JsonIgnore]
        public Territory Stronghold
        {
            get { return Game.Map.TerritoryLookup.Find(_strongholdId); }
            set { _strongholdId = Game.Map.TerritoryLookup.GetId(value); }
        }

        public override Message Validate()
        {
            if (!ValidStrongholds(Game, Player).Contains(Stronghold)) return Message.Express("Invalid stronghold");
            if (!ValidAmbassadors(Player).Contains(Faction)) return Message.Express("Ambassador not available");
            
            return null;
        }

        public static IEnumerable<Territory> ValidStrongholds(Game g, Player p)
        {
            var ally = g.GetPlayer(p.Ally);

            return g.Map.Territories(false).Where(t =>
                t.IsStronghold &&
                !g.IsInStorm(t) &&
                g.AmbassadorIn(t) == Faction.None);
        }

        public static IEnumerable<Faction> ValidAmbassadors(Player p) => p.Ambassadors;
        
        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public static bool IsApplicable(Game g, Player p) => 
            g.CurrentPhase == Phase.ResurrectionReport && 
            p.Resources > g.AmbassadorsPlacedThisTurn &&
            !g.Prevented(FactionAdvantage.PinkAmbassadors) &&
            ValidAmbassadors(p).Any() && 
            ValidStrongholds(g, p).Any();

        public override Message GetMessage()
        {
            if (Passed)
            {
                return Message.Express(Initiator, " don't place an ambassador"); 
            }
            else
            {
                return Message.Express(Initiator, " place an ambassador in ", Stronghold);
            }
        }
    }
}
