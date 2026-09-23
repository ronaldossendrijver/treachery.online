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
public class AmbassadorActivatedTests
{
    [TestMethod]
    public void FremenAmbassadorOnlyAllowsVisibleAttachedLocations()
    {
        var game = new Game();
        var player = new Player(game, Faction.Pink);
        game.Players.Add(player);

        var undiscoveredStronghold = game.Map.Jacurutu;
        var hiddenMobileStronghold = game.Map.HiddenMobileStronghold;

        var targetsBeforeDiscovery = AmbassadorActivated.ValidYellowTargets(game, player).ToArray();

        Assert.IsFalse(targetsBeforeDiscovery.Contains(undiscoveredStronghold));
        Assert.IsFalse(targetsBeforeDiscovery.Contains(hiddenMobileStronghold));

        undiscoveredStronghold.PointAt(game, game.Map.TheGreatFlat);
        hiddenMobileStronghold.PointAt(game, game.Map.Arrakeen);

        var targetsAfterDiscovery = AmbassadorActivated.ValidYellowTargets(game, player).ToArray();

        Assert.IsTrue(targetsAfterDiscovery.Contains(undiscoveredStronghold));
        Assert.IsTrue(targetsAfterDiscovery.Contains(hiddenMobileStronghold));
    }
}
