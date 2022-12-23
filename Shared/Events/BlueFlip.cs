/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class BlueFlip : GameEvent
    {
        public BlueFlip(Game game) : base(game)
        {
        }

        public BlueFlip()
        {
        }

        public bool AsAdvisors { get; set; }

        public override Message Validate()
        {
            if (Initiator != Faction.Blue) return Message.Express("Your faction can't flip");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " flip to ", AsAdvisors ? (object)FactionSpecialForce.Blue : FactionForce.Blue);
        }

        public Message GetDynamicMessage(Game g)
        {
            var territory = GetTerritory(g);
            var blue = g.GetPlayer(Faction.Blue);
            bool hasAdvisorsThere = blue != null && blue.SpecialForcesIn(territory) > 0;

            return Message.Express(
                Initiator,
                hasAdvisorsThere ^ AsAdvisors ? " become " : " stay as ",
                AsAdvisors ? (object)FactionSpecialForce.Blue : FactionForce.Blue,
                " in ",
                territory);
        }

        public static Territory GetTerritory(Game g) => g.LastIntrusionTrigger.Territory;
    }
}
