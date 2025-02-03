using System;
using System.Text.RegularExpressions;

namespace Treachery.Shared;

public static class Utilities
{
    public static List<T> CloneList<T>(List<T> toClone) where T : ICloneable
    {
        var result = new List<T>();

        foreach (var item in toClone) result.Add((T)item.Clone());

        return result;
    }

    public static Dictionary<X, Y> CloneObjectDictionary<X, Y>(Dictionary<X, Y> toClone) where Y : ICloneable
    {
        var result = new Dictionary<X, Y>();

        foreach (var item in toClone) result.Add(item.Key, (Y)item.Value.Clone());

        return result;
    }

    public static Dictionary<X, Y> CloneEnumDictionary<X, Y>(Dictionary<X, Y> toClone) where Y : struct, Enum
    {
        var result = new Dictionary<X, Y>();

        foreach (var item in toClone) result.Add(item.Key, item.Value);

        return result;
    }

    private static readonly JsonSerializerOptions Options = new() { IncludeFields = true };

    private const string NewtonsoftIndicator = "Treachery.Shared";
    private const string Pattern = "\"\\$type\":\"Treachery\\.Shared\\.([A-Za-z0-9_\\[\\]]+), Treachery\\.Shared\",";

    public static T Deserialize<T>(string serialized)
    {
        if (serialized.Substring(0, 100).Contains(NewtonsoftIndicator))
        {
            var adjustedSerialized = Regex.Replace(serialized, Pattern, match =>
            {
                string typeName = match.Groups[1].Value;
                if (IsGameEvent(typeName))
                {
                    var newValue = "\"$type\":\"" + match.Value + "\",";
                    
                    return newValue;
                }
                else
                {
                    return string.Empty; // Remove the match
                }
            });
            return JsonSerializer.Deserialize<T>(adjustedSerialized, Options);
        }
        
        
        return JsonSerializer.Deserialize<T>(serialized, Options);

    }
    
    private static bool IsGameEvent(string typeName)
    {
        var foundType = Type.GetType($"Treachery.Shared.{typeName}");
        return foundType != null && !foundType.IsAbstract && !foundType.IsInterface && foundType.IsSubclassOf(typeof(GameEvent));
    }

    public static string Serialize<T>(T value)
        => JsonSerializer.Serialize(value, Options);
}