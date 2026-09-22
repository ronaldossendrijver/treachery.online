/*
 * Copyright (C) 2020-2026 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Treachery.Bots;
using Treachery.Shared;
using Treachery.Shared.Model;

namespace Treachery.Shared.Test;

[TestClass]
public class BotShipmentTests
{
    [TestMethod]
    public void FremenShipAnEvenNumberToVacantStrongholdInCurrentStorm()
    {
        var state = GameState.Load(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", "issue-6.json")));
        state.Events = state.Events.Take(state.Events.Count() - 2);

        var loadError = Game.TryLoad(state, new Participation(), false, true, out var game);

        Assert.IsNull(loadError);
        Assert.IsNotNull(game);
        var loadedGame = game;

        var fremen = loadedGame.GetPlayer(Faction.Yellow);
        Assert.IsNotNull(fremen);
        var bot = new ClassicBot(loadedGame, fremen, BotParameters.GetDefaultParameters(Faction.Yellow));
        var action = bot.DetermineLowPriorityInPhaseAction(new List<Type> { typeof(Shipment) });

        Assert.IsInstanceOfType<Shipment>(action);
        var shipment = (Shipment)action;
        Assert.AreSame(loadedGame.Map.SietchTabr, shipment.To);
        Assert.IsFalse(shipment.Passed);
        Assert.AreEqual(2, shipment.ForceAmount + shipment.SpecialForceAmount + shipment.SmuggledAmount + shipment.SmuggledSpecialAmount);
    }
}
