/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class TestingStationUsed : GameEvent
    {
        #region Construction

        public TestingStationUsed(Game game) : base(game)
        {
        }

        public TestingStationUsed()
        {
        }

        #endregion Construction

        #region Properties

        public int ValueAdded { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (ValueAdded != -1 && ValueAdded != 1) return Message.Express("Invalid amount");

            return null;
        }

        public static bool CanBePlayed(Game g, Player p) => g.CurrentPhase == Phase.MetheorAndStormSpell && p.Occupies(g.Map.TestingStation) && !g.CurrentTestingStationUsed;

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();
            Game.Stone(Milestone.WeatherControlled);
            Game.CurrentTestingStationUsed = true;
            Game.NextStormMoves += ValueAdded;
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use ", DiscoveryToken.TestingStation, " to ", ValueAdded == 1 ? " add 1 to " : " subtract 1 from ", " Storm movement");
        }

        #endregion Execution
    }
}
