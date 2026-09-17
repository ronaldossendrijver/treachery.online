/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Treachery.Bots;
using Treachery.Shared;
using Treachery.Shared.Model;

namespace Treachery.Shared.Test;

[TestClass]
public class FaceDancerReplacedTests
{
    [TestMethod]
    public void BotCanChooseReplacementWhenSavedAllyIsMissing()
    {
        var game = new Game();
        var purple = new Player(game, Faction.Purple) { Ally = Faction.Green };
        game.Players.Add(purple);

        var deadLeader = LeaderManager.Leaders.First(leader => leader.Faction == Faction.Green);
        purple.FaceDancers.Add(deadLeader);
        game.LeaderState[deadLeader].Kill(game);

        var bot = new ClassicBot(game, purple, BotParameters.GetDefaultParameters(Faction.Purple));
        var action = bot.DetermineLowPriorityInPhaseAction(new List<System.Type> { typeof(FaceDancerReplaced) });

        Assert.IsInstanceOfType<FaceDancerReplaced>(action);
        Assert.AreSame(deadLeader, ((FaceDancerReplaced)action).SelectedDancer);
    }
}
