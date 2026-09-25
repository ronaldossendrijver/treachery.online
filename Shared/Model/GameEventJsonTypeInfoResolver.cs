/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;

namespace Treachery.Shared;

/// <summary>
/// Configures System.Text.Json so that non-public (including readonly) instance fields of GameEvents are serialized and deserialized.
/// Compiler generated backing fields and fields marked with [JsonIgnore] are skipped.
/// </summary>
public static class GameEventJsonTypeInfoResolver
{
    public static JsonSerializerOptions Configure(JsonSerializerOptions options)
    {
        options.IncludeFields = true;
        options.TypeInfoResolver = (options.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver()).WithAddedModifier(IncludeNonPublicFields);
        
        return options;
    }

    private static void IncludeNonPublicFields(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object || !typeof(GameEvent).IsAssignableFrom(typeInfo.Type)) return;

        for (var type = typeInfo.Type; type != null && type != typeof(object); type = type.BaseType)
        {
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (field.IsDefined(typeof(CompilerGeneratedAttribute)) || field.IsDefined(typeof(JsonIgnoreAttribute))) continue;

                var name = field.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? field.Name;

                var existing = typeInfo.Properties.FirstOrDefault(p => p.Name == name);
                if (existing != null)
                {
                    //Already included (e.g. by [JsonInclude]); make sure readonly fields can still be set
                    if (Equals(existing.AttributeProvider, field) && existing.Set == null) existing.Set = field.SetValue;
                    continue;
                }

                var property = typeInfo.CreateJsonPropertyInfo(field.FieldType, name);
                property.Get = field.GetValue;
                property.Set = field.SetValue;
                property.AttributeProvider = field;
                typeInfo.Properties.Add(property);
            }
        }
    }
}


