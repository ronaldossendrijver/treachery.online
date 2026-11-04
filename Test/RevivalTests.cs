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
public class RevivalTests
{
    [TestMethod]
    [DataRow(169, false, true)]
    [DataRow(195, false, true)]
    [DataRow(196, false, false)]
    [DataRow(196, true, false)]
    public void RevivingAlreadyRevivedLeaderPreservesHistoricalValidation(int version, bool secondEdition, bool allowed)
    {
        var game = new Game(version, new Participation());
        if (secondEdition) game.Rules.AddRange(Game.RulesetDefinition[Ruleset.SecondEdition]);
        var player = new Player(game, Faction.Purple) { Resources = 100, ForcesKilled = 4 };
        player.AssignLeaders(game);
        game.Players.Add(player);
        var leader = player.Leaders.First();
        game.LeaderState[leader].DeathCounter = 1;
        var revival = new Revival(game, player.Faction) { Hero = leader, AmountOfForces = 1 };

        Assert.IsNull(revival.Validate());
        game.LeaderState[leader].Revive();

        Assert.AreEqual(allowed, revival.Validate() == null);
    }

    [TestMethod]
    [DataRow(169, true)]
    [DataRow(195, true)]
    [DataRow(196, false)]
    public void NegativeRevivalAmountPreservesHistoricalValidation(int version, bool allowed)
    {
        var game = new Game(version, new Participation());
        var player = new Player(game, Faction.Black) { Resources = 100 };
        game.Players.Add(player);
        var revival = new Revival(game, player.Faction) { AmountOfForces = -1 };

        Assert.AreEqual(allowed, revival.Validate() == null);
    }

    [TestMethod]
    public void NonPurplePlayerCannotReviveMultipleLeadersInOneTurn()
    {
        var game = new Game();
        var player = new Player(game, Faction.Black)
        {
            Resources = 100
        };
        game.Players.Add(player);
        game.LeaderRevivalsThisTurn[player.Faction] = 1;

        var revival = new Revival(game, player.Faction)
        {
            Hero = LeaderManager.Leaders.First(leader => leader.Faction == player.Faction)
        };

        Assert.IsNotNull(revival.Validate());
    }

    [TestMethod]
    public void NonPurplePlayerCanReviveForcesAfterRevivingLeaderInSameTurn()
    {
        var game = new Game();
        var player = new Player(game, Faction.Black)
        {
            Resources = 100,
            ForcesKilled = 5
        };
        game.Players.Add(player);
        game.LeaderRevivalsThisTurn[player.Faction] = 1;

        var revival = new Revival(game, player.Faction)
        {
            Hero = null,
            AmountOfForces = 1
        };

        Assert.IsNull(revival.Validate());
    }

    [TestMethod]
    public void AutoClaimedSpecialForcePreventsManualRevivalOfAnotherSpecialForce()
    {
        var game = new Game();
        var player = new Player(game, Faction.Red)
        {
            SpecialForcesKilled = 2
        };
        game.Players.Add(player);

        game.ClaimFreeRevival(player);

        var revival = new Revival(game, player.Faction)
        {
            AmountOfSpecialForces = 1
        };

        Assert.IsNotNull(revival.Validate());
        Assert.AreEqual(1, player.SpecialForcesKilled);
    }
}
