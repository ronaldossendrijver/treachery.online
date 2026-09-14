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
using Treachery.Shared;

namespace Treachery.Test;

[TestClass]
public class GameSummaryTests
{
    [TestMethod]
    public void ReplayKeepsPreselectedHarkonnenPlayer()
    {
        var game = CreateGame();
        var harkonnen = game.Players.Single(p => p.Seat == 0);

        new FactionSelected(game)
        {
            InitiatorPlayerName = harkonnen.Name,
            Seat = harkonnen.Seat,
            Faction = Faction.Black
        }.Execute(false, true);

        new EndPhase(game, Faction.Black).Execute(false, true);

        var replay = new Game(game.Version, game.Participation);
        foreach (var gameEvent in game.History)
        {
            var replayEvent = gameEvent.Clone();
            replayEvent.Initialize(replay);
            replayEvent.Execute(false, true);
        }

        Assert.AreEqual(1, replay.Players.Count(p => p.Faction == Faction.Black));
        Assert.AreEqual(Faction.Black, replay.GetPlayerBySeat(0)?.Faction);
        Assert.AreEqual(8, replay.GetPlayer(Faction.Black)?.MaximumNumberOfCards);
        Assert.AreEqual(replay.Players.Count, replay.Players.Select(p => p.Faction).Distinct().Count());
    }

    private static Game CreateGame()
    {
        var participation = new Participation
        {
            PlayerNames = new Dictionary<int, string>
            {
                [1] = "Harkonnen player",
                [2] = "Other player"
            },
            SeatedPlayers = new Dictionary<int, int>
            {
                [1] = 0,
                [2] = 1
            }
        };

        var game = new Game(Game.LatestVersion, participation);
        new EstablishPlayers(game, Faction.None)
        {
            Seed = 122,
            Settings = new GameSettings
            {
                NumberOfPlayers = 2,
                MaximumTurns = 10,
                InitialRules = [Rule.PlayersChooseFactions],
                AllowedFactionsInPlay = [Faction.Black, Faction.Green]
            }
        }.Execute(false, true);

        return game;
    }
}
