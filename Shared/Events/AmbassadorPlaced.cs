/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
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

        public Ambassador Ambassador;

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
            if (!ValidAmbassadors(Player).Contains(Ambassador)) return Message.Express("Ambassador not available");

            return null;
        }

        public static IEnumerable<Territory> ValidStrongholds(Game g, Player p)
        {
            var ally = g.GetPlayer(p.Ally);

            return g.Map.Territories(false).Where(t =>
                t.IsVisible &&
                t.IsStronghold &&
                !g.IsInStorm(t) &&
                g.AmbassadorIn(t) == Ambassador.None);
        }

        public static IEnumerable<Ambassador> ValidAmbassadors(Player p) => p.Ambassadors;

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
            return Message.Express(Initiator, " station the ", Ambassador, " ambassador in ", Stronghold);
        }
    }
}
