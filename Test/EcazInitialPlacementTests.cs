/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Reflection;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Treachery.Shared;
using Treachery.Shared.Model;

namespace Treachery.Test;

[TestClass]
public class EcazInitialPlacementTests
{
    [TestMethod]
    public void Version187DefersEcazForcesToInitialPlacementEvent()
    {
        var (game, ecaz) = CreateEcazGame(187);

        SetUpEcaz(game, ecaz);

        Assert.AreEqual(0, ecaz.ForcesIn(game.Map.ImperialBasin));
        Assert.AreEqual(12, ecaz.Resources);
    }

    [TestMethod]
    public void HistoricalVersionsKeepEcazForcesInMiddleSector()
    {
        var (game, ecaz) = CreateEcazGame(186);

        SetUpEcaz(game, ecaz);

        Assert.AreEqual(6, ecaz.ForcesIn(game.Map.ImperialBasin.MiddleLocation));
    }

    [TestMethod]
    public void ValidLocationsAreTheThreeImperialBasinSectors()
    {
        var game = new Game(187, new Participation());

        CollectionAssert.AreEquivalent(
            game.Map.ImperialBasin.Locations.ToArray(),
            EcazInitialPlacement.ValidLocations(game).ToArray());
        Assert.AreEqual(3, EcazInitialPlacement.ValidLocations(game).Count());
    }

    [TestMethod]
    public void PlacementMovesSixForcesToSelectedSector()
    {
        var game = new Game(187, new Participation());
        var ecaz = new Player(game, Faction.Pink);
        game.Players.Add(ecaz);
        game.Players.Add(new Player(game, Faction.Cyan));
        ecaz.InitializeHomeworld(game.Map.Homeworlds.First(w => w.Faction == Faction.Pink), 20, 0);
        var target = game.Map.ImperialBasin.Locations.First();

        var result = new EcazInitialPlacement(game, Faction.Pink) { Target = target }.Execute(false, true);

        Assert.IsNull(result);
        Assert.AreEqual(6, ecaz.ForcesIn(target));
        Assert.AreEqual(0, ecaz.ForcesIn(game.Map.ImperialBasin.MiddleLocation));
        Assert.AreEqual(Phase.CyanSettingUp, game.CurrentPhase);
    }

    [TestMethod]
    public void PlacementRejectsLocationsOutsideImperialBasin()
    {
        var game = new Game(187, new Participation());
        var ecaz = new Player(game, Faction.Pink);
        game.Players.Add(ecaz);

        var placement = new EcazInitialPlacement(game, Faction.Pink) { Target = game.Map.Arrakeen };

        Assert.IsNotNull(placement.Validate());
    }

    private static (Game Game, Player Ecaz) CreateEcazGame(int version)
    {
        var game = new Game(version, new Participation());
        var ecaz = new Player(game, Faction.Pink);
        game.Players.Add(ecaz);
        return (game, ecaz);
    }

    private static void SetUpEcaz(Game game, Player ecaz)
    {
        InvokeSetupMethod(game, "SetupPlayerHomeworld", ecaz);
        InvokeSetupMethod(game, "SetupPlayerSpiceAndForcesOnPlanet", ecaz);
    }

    private static void InvokeSetupMethod(Game game, string methodName, Player player)
    {
        typeof(Game).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(game, [player]);
    }
}
