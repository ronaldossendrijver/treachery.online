/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Treachery.Shared;
using Treachery.Shared.Model;

namespace Treachery.Test;

[TestClass]
public class TakeLossesTests
{
    [TestMethod]
    public void GraduateBattleLossesCanBeDistributedAcrossForceTypes()
    {
        var game = new Game();
        var player = new Player(game, Faction.Red);
        game.Players.Add(player);
        game.LossesToTake.Add(new LossToTake
        {
            Faction = player.Faction,
            ForceOwner = player.Faction,
            Location = game.Map.Carthag,
            Amount = 2,
            MaximumForceAmount = 2,
            MaximumSpecialForceAmount = 3,
            ForcesToRemain = 1,
            IsBattleLoss = true
        });

        Assert.IsNull(new TakeLosses(game, player.Faction) { ForceAmount = 2, SpecialForceAmountToRemain = 1 }.Validate());
        Assert.IsNull(new TakeLosses(game, player.Faction) { SpecialForceAmount = 2, ForceAmountToRemain = 1 }.Validate());
    }

    [TestMethod]
    public void GraduateBattleLossesRequireTheExactSavedTotal()
    {
        var game = new Game();
        var player = new Player(game, Faction.Red);
        game.Players.Add(player);
        game.LossesToTake.Add(new LossToTake
        {
            Faction = player.Faction,
            ForceOwner = player.Faction,
            Location = game.Map.Carthag,
            Amount = 2,
            MaximumForceAmount = 2,
            MaximumSpecialForceAmount = 3,
            ForcesToRemain = 1,
            IsBattleLoss = true
        });

        Assert.IsNotNull(new TakeLosses(game, player.Faction) { ForceAmount = 1, ForceAmountToRemain = 1 }.Validate());
        Assert.IsNotNull(new TakeLosses(game, player.Faction) { ForceAmount = 2, SpecialForceAmount = 1, ForceAmountToRemain = 1 }.Validate());
    }

    [TestMethod]
    public void GraduateBattleLossesRequireAValidForceToRemain()
    {
        var game = new Game();
        var player = new Player(game, Faction.Red);
        game.Players.Add(player);
        game.LossesToTake.Add(new LossToTake
        {
            Faction = player.Faction,
            ForceOwner = player.Faction,
            Location = game.Map.Carthag,
            Amount = 2,
            MaximumForceAmount = 2,
            MaximumSpecialForceAmount = 3,
            ForcesToRemain = 1,
            IsBattleLoss = true
        });

        Assert.IsNotNull(new TakeLosses(game, player.Faction) { ForceAmount = 2 }.Validate());
        Assert.IsNotNull(new TakeLosses(game, player.Faction) { ForceAmount = 2, ForceAmountToRemain = 1 }.Validate());
        Assert.IsNull(new TakeLosses(game, player.Faction) { ForceAmount = 2, SpecialForceAmountToRemain = 1 }.Validate());
    }
}
