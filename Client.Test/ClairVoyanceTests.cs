/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Treachery.Shared;

namespace Treachery.Client.Test;

[TestClass]
public class ClairVoyanceTests
{
    [TestMethod]
    public void GenericCardTypesAreAvailableForOwnershipQuestion()
    {
        var game = new Game(Game.LatestVersion, new Participation());

        var ownershipTypes = ClairVoyancePlayed.ValidCardTypes(game, ClairvoyanceQuestion.HasCardTypeInHand);
        var battleTypes = ClairVoyancePlayed.ValidCardTypes(game, ClairvoyanceQuestion.CardTypeInBattle);

        CollectionAssert.Contains(ownershipTypes.ToList(), TreacheryCardType.Weapon);
        CollectionAssert.Contains(ownershipTypes.ToList(), TreacheryCardType.Defense);
        CollectionAssert.DoesNotContain(battleTypes.ToList(), TreacheryCardType.Weapon);
        CollectionAssert.DoesNotContain(battleTypes.ToList(), TreacheryCardType.Defense);
    }

    [TestMethod]
    [DataRow(TreacheryCardType.Projectile, TreacheryCardType.Weapon, true)]
    [DataRow(TreacheryCardType.Shield, TreacheryCardType.Defense, true)]
    [DataRow(TreacheryCardType.Chemistry, TreacheryCardType.Weapon, true)]
    [DataRow(TreacheryCardType.Chemistry, TreacheryCardType.Defense, true)]
    [DataRow(TreacheryCardType.Useless, TreacheryCardType.Weapon, false)]
    [DataRow(TreacheryCardType.Useless, TreacheryCardType.Defense, false)]
    public void GenericCardTypesMatchCardClassification(
        TreacheryCardType cardType,
        TreacheryCardType asked,
        bool expected)
    {
        Assert.AreEqual(expected, ClairVoyancePlayed.IsInScopeOf(false, cardType, asked));
        Assert.AreEqual(expected, ClairVoyanceAnswered.IsQuestionedBy(false, cardType, asked));
    }
}
