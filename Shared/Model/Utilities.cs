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
    private const string ToReplace1 = "\"$type\" : \"Treachery.Shared.GameState, Treachery.Shared\",";
    private const string ToReplace2 = "\"$type\" : \"Treachery.Shared.GameEvent[], Treachery.Shared\",";
    private const string Pattern = @"Treachery\.Shared\.([A-Za-z0-9_]+)|, Treachery\.Shared";

    public static T Deserialize<T>(string serialized)
    {
        if (serialized.Substring(0, 100).Contains(NewtonsoftIndicator))
        {
            var cleaned = serialized.Replace(ToReplace1, string.Empty, StringComparison.InvariantCultureIgnoreCase).Replace(ToReplace2, string.Empty, StringComparison.InvariantCultureIgnoreCase);
            string adjustedSerialized =  Regex.Replace(cleaned, Pattern, match => match.Groups[1].Success ? match.Groups[1].Value : "");
            return JsonSerializer.Deserialize<T>(adjustedSerialized, Options);
        }
        
        
        return JsonSerializer.Deserialize<T>(serialized, Options);

    }

    public static string Serialize<T>(T value)
        => JsonSerializer.Serialize(value, Options);
}