/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Reflection;

namespace Treachery.Test;

[TestClass]
public class BotTerrorTests
{
    [TestMethod]
    public void BotPlantsAvailableTokenAfterRobberyWasRevealed()
    {
        var game = new Game();
        var cyan = new Player(game, Faction.Cyan) { Resources = 10 };
        var opponent = new Player(game, Faction.Red) { Resources = 20 };
        game.Players.AddRange([cyan, opponent]);
        game.UnplacedTerrorTokens.AddRange([TerrorType.Assassination, TerrorType.Extortion]);
        cyan.AddForces(game.Map.Arrakeen, 1, false);
        SetCurrentMainPhase(game, MainPhase.Contemplate);

        var bot = new ClassicBot(game, cyan, BotParameters.GetDefaultParameters(Faction.Cyan));
        var action = bot.DetermineLowPriorityInPhaseAction([typeof(TerrorPlanted)]);

        Assert.IsInstanceOfType<TerrorPlanted>(action);
        var terrorPlanted = (TerrorPlanted)action;
        Assert.IsFalse(terrorPlanted.Passed);
        Assert.AreNotEqual(TerrorType.Robbery, terrorPlanted.Type);
        Assert.Contains(terrorPlanted.Type, game.UnplacedTerrorTokens);
        Assert.IsNull(terrorPlanted.Validate());
    }

    private static void SetCurrentMainPhase(Game game, MainPhase phase)
    {
        typeof(Game).GetProperty(
                nameof(Game.CurrentMainPhase),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(game, phase);
    }
}
