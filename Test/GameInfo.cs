/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using Treachery.Shared;

namespace Treachery.Test
{
    public class GameInfo
    {
        public static IEnumerable<string> GetHeaders()
        {
            var result = new List<string>();

            //Game info
            result.Add("GameTurn");
            result.Add("GamePhase");

            return result;
        }

        public static IEnumerable<int> GetState(Game game)
        {
            var result = new List<int>();

            //Game info
            result.Add(game.CurrentTurn);
            result.Add((int)game.CurrentPhase);

            var factions = EstablishPlayers.AvailableFactions();

            //All locations
            foreach (var location in game.Map.Locations(true))
            {
                //Spice


                //all factions, normal and special forces
                foreach (var faction in factions)
                {

                }
            }



            return result;
        }

        
    }
}
