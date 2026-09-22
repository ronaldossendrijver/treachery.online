/*
 * Copyright (C) 2020-2026 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Treachery.Bots;
using Treachery.Shared.Model;

namespace Treachery.Shared.Test;

[TestClass]
public class BotClairvoyanceTests
{
    [TestMethod]
    public void BotKeepsClairvoyanceAfterFillingItsHandDuringBidding()
    {
        var game = new Game();
        var player = new Player(game, Faction.Red);
        var opponent = new Player(game, Faction.Yellow);
        game.Players.AddRange([player, opponent]);
        player.Leaders.Add(LeaderManager.Leaders.First(leader => leader.Faction == player.Faction));
        player.TreacheryCards.AddRange(TreacheryCardManager.Items
            .Where(card => card.Type != TreacheryCardType.Clairvoyance)
            .Take(player.MaximumNumberOfCards - 1));
        player.TreacheryCards.Add(TreacheryCardManager.Items.First(card => card.Type == TreacheryCardType.Clairvoyance));
        SetCurrentPhase(game, Phase.Bidding);

        var bot = new ClassicBot(game, player, BotParameters.GetDefaultParameters(player.Faction));
        var action = bot.DetermineLowPriorityInPhaseAction([typeof(ClairVoyancePlayed)]);

        Assert.IsFalse(player.HasRoomForCards);
        Assert.IsNull(action);
    }

    private static void SetCurrentPhase(Game game, Phase phase)
    {
        typeof(Game).GetProperty(
                nameof(Game.CurrentPhase),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(game, phase);
    }
}
