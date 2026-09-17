/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Treachery.Shared.Test;

[TestClass]
public class ShipmentTests
{
    [TestMethod]
    [DataRow(Faction.White, 3, 1)]
    [DataRow(Faction.White, -1, 0)]
    [DataRow(Faction.Black, 3, 0)]
    [DataRow(Faction.Black, 5, 0)]
    public void NoFieldShipmentOnlyAddsSpecialForceForRichese(Faction faction, int noFieldValue, int expected)
    {
        Assert.AreEqual(expected, Shipment.DefaultNoFieldSpecialForceAmount(faction, noFieldValue));
    }
}

