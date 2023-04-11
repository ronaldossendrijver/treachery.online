/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class PerformSetup : PlacementEvent
    {
        #region Construction

        public PerformSetup(Game game) : base(game)
        {
        }

        public PerformSetup()
        {
        }

        #endregion Construction

        #region Properties

        public int Resources { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            var faction = Game.NextFactionToPerformCustomSetup;
            var p = Game.GetPlayer(faction);
            int numberOfSpecialForces = ForceLocations.Values.Sum(b => b.AmountOfSpecialForces);
            if (numberOfSpecialForces > p.SpecialForcesInReserve) return Message.Express("Too many ", p.SpecialForce);

            int numberOfForces = ForceLocations.Values.Sum(b => b.AmountOfForces);
            if (numberOfForces > p.ForcesInReserve) return Message.Express("Too many ", p.Force);

            return null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            var faction = Game.NextFactionToPerformCustomSetup;
            var player = GetPlayer(faction);

            foreach (var fl in ForceLocations)
            {
                var location = fl.Key;
                player.ShipForces(location, fl.Value.AmountOfForces);
                player.ShipSpecialForces(location, fl.Value.AmountOfSpecialForces);
            }

            player.Resources = Resources;

            Log(faction, " initial positions set, starting with ", Payment.Of(Resources));
            Game.HasActedOrPassed.Add(faction);

            if (Game.Players.Count == Game.HasActedOrPassed.Count)
            {
                Game.Enter(Game.TreacheryCardsBeforeTraitors, Game.EnterStormPhase, Game.DealStartingTreacheryCards);
            }
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " initial positions and ", Concept.Resource, " (", Payment.Of(Resources), ") determined");
        }

        #endregion Execution
    }
}
