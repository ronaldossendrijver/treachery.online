/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
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

        public override string Validate()
        {
            if (Initiator != Faction.Blue) return "Your faction can't flip";
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} flip to {1}.", Initiator, AsAdvisors ? "advisors" : "fighters");
        }

        public Message GetDynamicMessage(Game g)
        {
            var territory = GetTerritory(g);
            var blue = g.GetPlayer(Faction.Blue);
            bool hasAdvisorsThere = blue != null && blue.SpecialForcesIn(territory) > 0;

            return new Message(Initiator, "{0} {1} {2}.", 
                Initiator,
                hasAdvisorsThere ^ AsAdvisors ? "become" : "stay as", 
                AsAdvisors ? "advisors" : "fighters");
        }

        public static Territory GetTerritory(Game g)
        {
            return g.LastShippedOrMovedTo.Territory;
        }
    }
}
