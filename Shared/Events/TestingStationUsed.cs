/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class TestingStationUsed : GameEvent
    {
        public TestingStationUsed(Game game) : base(game)
        {
        }

        public TestingStationUsed()
        {
        }

        public int ValueAdded { get; set; }

        public override Message Validate()
        {
            if (ValueAdded != -1 && ValueAdded != 1) return Message.Express("Invalid amount");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use ", DiscoveryToken.TestingStation, " to ", ValueAdded == 1 ? " add 1 to " : " subtract 1 from ", " Storm movement");
        }

        public static bool CanBePlayed(Game g, Player p) => g.CurrentPhase == Phase.MetheorAndStormSpell && p.Occupies(g.Map.TestingStation) && !g.CurrentTestingStationUsed;
    }
}
