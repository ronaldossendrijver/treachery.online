/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using Treachery.Bots;
using Treachery.Shared;

namespace Treachery.Test;

[TestClass]
public class BotClairvoyanceTests
{
    [TestMethod]
    [DataRow(Faction.Green, Faction.None)]
    [DataRow(Faction.Blue, Faction.None)]
    [DataRow(Faction.Orange, Faction.Green)]
    [DataRow(Faction.Orange, Faction.Blue)]
    public void BotWaitsForOpponentFactionPowerAfterOpponentSubmitsBattlePlan(
        Faction opponentFaction,
        Faction opponentAlly)
    {
        var game = new Game();
        var botPlayer = new Player(game, Faction.Red);
        var opponent = new Player(game, opponentFaction);
        game.Players.AddRange([botPlayer, opponent]);
        if (opponentAlly != Faction.None)
        {
            game.Players.Add(new Player(game, opponentAlly));
            opponent.Ally = opponentAlly;
            var permission = opponentAlly == Faction.Green
                ? nameof(Game.GreenSharesPrescience)
                : nameof(Game.BlueAllowsUseOfVoice);
            SetProperty(game, permission, true);
        }

        var battle = new BattleInitiated(game, Faction.Red)
        {
            Target = opponentFaction,
            Territory = game.Map.Arrakeen.Territory
        };
        SetProperty(game, nameof(Game.CurrentBattle), battle);
        SetProperty(game, nameof(Game.CurrentPhase), Phase.BattlePhase);
        SetProperty(game, nameof(Game.DefenderPlan), new Battle(game, opponentFaction));
        opponent.TreacheryCards.Add(TreacheryCardManager.Items.First(card => card.IsWeapon));
        botPlayer.TreacheryCards.Add(TreacheryCardManager.Items.First(card => card.IsPoisonWeapon));

        var bot = new TestBot(game, botPlayer);

        Assert.IsTrue(Prescience.MayUsePrescience(game, opponent) || Voice.MayUseVoice(game, opponent));
        Assert.IsNull(bot.DetermineClairvoyanceForTest());
    }

    private static void SetProperty(object instance, string propertyName, object value)
    {
        typeof(Game).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(instance, value);
    }

    private sealed class TestBot(Game game, Player player)
        : ClassicBot(game, player, BotParameters.GetDefaultParameters(player.Faction))
    {
        public ClairVoyancePlayed? DetermineClairvoyanceForTest() => DetermineClairvoyance();
    }
}
