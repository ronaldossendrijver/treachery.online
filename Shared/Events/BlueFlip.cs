/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;

namespace Treachery.Shared
{
    public class BlueFlip : GameEvent
    {
        #region Construction

        public BlueFlip(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public BlueFlip()
        {
        }

        #endregion Construction

        #region Properties

        public bool AsAdvisors { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (Initiator != Faction.Blue) return Message.Express("Your faction can't flip");

            return null;
        }

        public static Territory GetTerritory(Game g) => g.LastBlueIntrusion.Territory;

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log(GetDynamicMessage());

            Player.FlipForces(Game.LastShipmentOrMovement.To.Territory, AsAdvisors);

            if (Game.Version >= 102) Game.FlipBeneGesseritWhenAloneOrWithPinkAlly();

            Game.DequeueIntrusion(IntrusionType.BlueIntrusion);
            Game.DetermineNextShipmentAndMoveSubPhase();
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " flip to ", AsAdvisors ? FactionSpecialForce.Blue : FactionForce.Blue);
        }

        public Message GetDynamicMessage()
        {
            var territory = GetTerritory(Game);
            bool hasAdvisorsThere = Player.SpecialForcesIn(territory) > 0;

            return Message.Express(
                Initiator,
                hasAdvisorsThere ^ AsAdvisors ? " become " : " stay as ",
                AsAdvisors ? FactionSpecialForce.Blue : FactionForce.Blue,
                " in ",
                territory);
        }

        #endregion Execution
    }
}
