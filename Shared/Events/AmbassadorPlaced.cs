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
        #region Construction

        public AmbassadorPlaced(Game game) : base(game)
        {
        }

        public AmbassadorPlaced()
        {
        }

        #endregion Construction

        #region Properties

        public Ambassador Ambassador { get; set; }

        public int _strongholdId;

        [JsonIgnore]
        public Territory Stronghold
        {
            get => Game.Map.TerritoryLookup.Find(_strongholdId);
            set => _strongholdId = Game.Map.TerritoryLookup.GetId(value);
        }

        #endregion Properties

        #region Validation

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

        public static bool IsApplicable(Game g, Player p) =>
            g.CurrentPhase == Phase.ResurrectionReport &&
            p.Resources > g.AmbassadorsPlacedThisTurn &&
            !g.Prevented(FactionAdvantage.PinkAmbassadors) &&
            ValidAmbassadors(p).Any() &&
            ValidStrongholds(g, p).Any();

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.AmbassadorsPlacedThisTurn++;
            Player.Resources -= Game.AmbassadorsPlacedThisTurn;
            Log(Initiator, " station the ", Ambassador, " ambassador in ", Stronghold, " for ", Payment.Of(Game.AmbassadorsPlacedThisTurn));
            Game.AmbassadorsOnPlanet.Add(Stronghold, Ambassador);
            Player.Ambassadors.Remove(Ambassador);
            Game.Stone(Milestone.AmbassadorPlaced);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " station the ", Ambassador, " ambassador in ", Stronghold);
        }

        #endregion Execution
    }
}
