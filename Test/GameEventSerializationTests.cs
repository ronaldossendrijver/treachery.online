/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Treachery.Test;

[TestClass]
public class GameEventSerializationTests
{
    [TestMethod]
    public void ScanForUndecoratedGetOnlyProperties()
    {
        var assembly = Assembly.GetAssembly(typeof(GameEvent));
        if (assembly == null)
            return;

        var gameEventType = typeof(GameEvent);
        foreach (var type in assembly.GetTypes().Where(myType => myType is { IsClass: true, IsAbstract: false } && myType.IsSubclassOf(gameEventType)))
        {
            var serializerAtt = gameEventType.GetCustomAttributes<JsonDerivedTypeAttribute>().FirstOrDefault(att => att.DerivedType == type);
            Assert.IsNotNull(serializerAtt,
                $"JsonDerivedType attribute missing for {type}");

            foreach (var prop in type.GetProperties().Where(x => !x.CanWrite))
            {
                var att = prop.GetCustomAttribute(typeof(JsonIgnoreAttribute));
                Assert.IsNotNull(att,
                    $"Get-only property {prop} of class {type} does not have the JsonIgnore attribute");
            }
        }
    }
}
