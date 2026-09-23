/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Treachery.Shared;

namespace Treachery.Test;

[TestClass]
public class KarmaTests
{
    [TestMethod]
    public void PlayingLastCardDuringGreySwapContinuesBidding()
    {
        var game = CreateGameInGreySwapPhase();
        var grey = game.GetPlayer(Faction.Grey)!;
        var karama = TreacheryCardManager.Get(23)!;
        grey.TreacheryCards.Add(karama);

        new Karma(game, Faction.Grey)
        {
            Card = karama,
            Prevented = FactionAdvantage.RedReceiveBid
        }.Execute(false, true);

        Assert.AreEqual(Phase.Bidding, game.CurrentPhase);
        Assert.IsFalse(grey.TreacheryCards.Contains(karama));
        Assert.IsTrue(game.TreacheryDiscardPile!.Items.Contains(karama));
    }

    [TestMethod]
    public void PlayingKarmaDuringGreySwapKeepsDecisionWhenAnotherCardRemains()
    {
        var game = CreateGameInGreySwapPhase();
        var grey = game.GetPlayer(Faction.Grey)!;
        var karama = TreacheryCardManager.Get(23)!;
        grey.TreacheryCards.Add(karama);
        grey.TreacheryCards.Add(TreacheryCardManager.Get(1)!);

        new Karma(game, Faction.Grey)
        {
            Card = karama,
            Prevented = FactionAdvantage.RedReceiveBid
        }.Execute(false, true);

        Assert.AreEqual(Phase.GreySwappingCard, game.CurrentPhase);
    }

    private static Game CreateGameInGreySwapPhase()
    {
        var game = new Game(Game.LatestVersion, new Participation());
        new EstablishPlayers(game, Faction.None)
        {
            Seed = 144,
            Settings = new GameSettings
            {
                NumberOfPlayers = 2,
                MaximumTurns = 10,
                InitialRules = [Rule.GreySwappingCardOnBid],
                AllowedFactionsInPlay = [Faction.Grey, Faction.Red]
            }
        }.Execute(false, true);

        SetProperty(game, nameof(Game.BidSequence), new PlayerSequence(game));
        SetProperty(game, "BiddingRoundWasStarted", true);
        SetProperty(game, nameof(Game.CurrentPhase), Phase.GreySwappingCard);
        return game;
    }

    private static void SetProperty<T>(Game game, string propertyName, T value)
    {
        typeof(Game).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(game, value);
    }
}
