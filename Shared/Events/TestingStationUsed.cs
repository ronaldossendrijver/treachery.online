/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

namespace Treachery.Shared
{
    public class TestingStationUsed : GameEvent
    {
        #region Construction

        public TestingStationUsed(Game game, Faction initiator) : base(game, initiator)
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
            Game.Stone(Milestone.WeatherControlled);
            Game.CurrentTestingStationUsed = true;
            Game.NextStormMoves += ValueAdded;
            Log(Initiator, " use ", DiscoveryToken.TestingStation, ": storm ", ValueAdded > 0 ? "increases" : "weakens", " to ", Game.NextStormMoves);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use ", DiscoveryToken.TestingStation, " to influence Storm movement");
        }

        #endregion Execution
    }
}
