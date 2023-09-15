/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
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
