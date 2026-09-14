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
}
