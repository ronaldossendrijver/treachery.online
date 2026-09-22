/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Treachery.Shared;
using Treachery.Shared.Model;

namespace Treachery.Test;

[TestClass]
public class TerrorPlantedTests
{
    [TestMethod]
    public void TerrorTokenCannotBeRemovedDuringMentatPause()
    {
        var game = CreateHighThresholdMoritaniGame();
        game.CurrentMainPhase = MainPhase.Contemplate;
        var terror = AddTerrorTokenToArrakis(game);

        var result = new TerrorPlanted(game, Faction.Cyan) { Type = terror }.Validate();

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void TerrorTokenCannotBeRemovedAtLowThreshold()
    {
        var game = new Game(Game.LatestVersion, new Participation());
        game.Rules.Add(Rule.Homeworlds);
        var moritani = new Player(game, Faction.Cyan);
        game.Players.Add(moritani);
        moritani.InitializeHomeworld(game.Map.Homeworlds.Single(world => world.World == World.Cyan), 0, 0);
        game.CurrentMainPhase = MainPhase.Collection;
        var terror = AddTerrorTokenToArrakis(game);

        var result = new TerrorPlanted(game, Faction.Cyan) { Type = terror }.Validate();

        Assert.IsNotNull(result);
    }

    private static Game CreateHighThresholdMoritaniGame()
    {
        var game = new Game(Game.LatestVersion, new Participation());
        game.Rules.Add(Rule.Homeworlds);
        var moritani = new Player(game, Faction.Cyan);
        game.Players.Add(moritani);
        var homeworld = game.Map.Homeworlds.Single(world => world.World == World.Cyan);
        moritani.InitializeHomeworld(homeworld, homeworld.Threshold, 0);
        return game;
    }

    private static TerrorType AddTerrorTokenToArrakis(Game game)
    {
        const TerrorType terror = TerrorType.Atomics;
        game.TerrorOnPlanet.Add(terror, game.Map.Arrakeen.Territory);
        return terror;
    }
}
