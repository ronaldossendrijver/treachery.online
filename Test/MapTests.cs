/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Treachery.Test;

[TestClass]
public class MapTests
{
    [TestMethod]
    public void ScanForMapErrors()
    {
        var map = new Map();
        var issueFound = false;

        foreach (var l in map.Locations(false))
        {
            var asymNeighbour = l.Neighbours.FirstOrDefault(neighbour => !neighbour.Neighbours.Contains(l));
            if (asymNeighbour != null)
            {
                issueFound = true;
                Console.WriteLine($"Asymmetrical: {DefaultSkin.Default.Describe(l)}[{l.Id}] <-> {DefaultSkin.Default.Describe(asymNeighbour)}[{asymNeighbour.Id}]");
            }
        }

        Assert.IsFalse(issueFound, "Asymmetrical neighbour relationship detected");
    }
}
