/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class PerformYellowSetup : PlacementEvent
    {
        #region Construction

        public PerformYellowSetup(Game game) : base(game)
        {
        }

        public PerformYellowSetup()
        {
        }

        #endregion Construction

        #region Validation

        public override Message Validate()
        {
            var p = Player;
            int numberOfSpecialForces = ForceLocations.Sum(fl => fl.Value.AmountOfSpecialForces);
            if (numberOfSpecialForces > p.SpecialForcesInReserve) return Message.Express("You only have ", p.SpecialForcesInReserve, " ", FactionSpecialForce.Yellow);

            int numberOfForces = ForceLocations.Sum(fl => fl.Value.AmountOfForces);
            if (numberOfForces + numberOfSpecialForces != 10) return Message.Express("Distribute 10 forces");

            return null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            foreach (var fl in ForceLocations)
            {
                var location = fl.Key;
                Player.ShipForces(location, fl.Value.AmountOfForces);
                Player.ShipSpecialForces(location, fl.Value.AmountOfSpecialForces);
            }

            Log();

            Game.Enter(
                IsPlaying(Faction.Blue) && PerformBluePlacement.BlueMayPlaceFirstForceInAnyTerritory(Game), Phase.BlueSettingUp,
                IsPlaying(Faction.Cyan), Phase.CyanSettingUp,
                Game.TreacheryCardsBeforeTraitors, Game.EnterStormPhase, Game.DealStartingTreacheryCards);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " have set up forces");
        }

        #endregion Execution
    }
}
