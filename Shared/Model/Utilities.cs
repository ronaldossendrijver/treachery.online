using System;
using System.Collections.Generic;

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
}