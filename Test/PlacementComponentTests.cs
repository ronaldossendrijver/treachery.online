/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Treachery.Client;
using Treachery.Client.GameEventComponents;
using Treachery.Shared;

namespace Treachery.Shared.Test;

[TestClass]
public class PlacementComponentTests
{
    [TestMethod]
    public void ReplacingGameClearsMovementSelection()
    {
        var service = DispatchProxy.Create<IGameService, GameServiceProxy>();
        var proxy = (GameServiceProxy)(object)service;
        var originalGame = new Game();
        proxy.Game = originalGame;

        var component = new TestPlacementComponent();
        component.SetClient(service);
        component.ApplyParameters();
        component.Select(originalGame.Map.Arrakeen);
        component.ApplyParameters();

        Assert.IsTrue(component.HasSelection);

        proxy.Game = new Game();
        component.ApplyParameters();

        Assert.IsFalse(component.HasSelection);
    }

    private sealed class TestPlacementComponent : PlacementComponent<Move>
    {
        protected override Move ConfirmedResult => new();
        protected override bool InformAboutCaravan => false;
        protected override string Title => "";
        protected override bool MayPass => true;

        public bool HasSelection => fromTerritory != null || toLocation != null || forces.Count != 0 || asAdvisors;

        public void ApplyParameters() => base.OnParametersSet();

        public void SetClient(IGameService service)
        {
            var type = GetType();
            var found = false;
            while (type != null)
            {
                var clientProperty = type.GetProperty("Client", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (clientProperty != null)
                {
                    clientProperty.SetValue(this, service);
                    found = true;
                }

                type = type.BaseType;
            }

            if (!found)
                throw new InvalidOperationException("Client injection property not found");
        }

        public void Select(Location location)
        {
            fromTerritory = location.Territory;
            toLocation = location;
            forces.Add(location, new Battalion(Faction.None, 1, 0, location));
            asAdvisors = true;
        }
    }

    private class GameServiceProxy : DispatchProxy
    {
        public Game? Game { get; set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod?.Name == "get_Game")
                return Game;

            throw new NotSupportedException(targetMethod?.Name);
        }
    }
}
