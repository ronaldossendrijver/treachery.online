﻿using System;

namespace Treachery.Shared;

public static class ExtensionMethods
{
    public static T HighestOrDefault<T>(this IEnumerable<T> source, Func<T, IComparable> selector)
    {
        if (source is null) return default;

        if (!source.Any()) return default;

        var best = source.Max(v => selector(v));

        return RandomOrDefault(source.Where(v => selector(v).Equals(best)));
    }

    public static T OneOfHighestNOrDefault<T>(this IEnumerable<T> source, Func<T, IComparable> selector, int n)
    {
        if (source is null || n <= 0) return default;

        List<T> toSelectFrom = new();
        var i = 0;
        foreach (var item in source.OrderByDescending(selector))
        {
            i++;
            toSelectFrom.Add(item);
            if (i == n) break;
        }

        if (i == 0) return default;

        return RandomOrDefault(toSelectFrom);
    }

    public static T OneOfLowestNOrDefault<T>(this IEnumerable<T> source, Func<T, IComparable> selector, int n)
    {
        if (source is null || n <= 0) return default;

        List<T> toSelectFrom = new();
        var i = 0;
        foreach (var item in source.OrderBy(selector))
        {
            i++;
            toSelectFrom.Add(item);
            if (i == n) break;
        }

        if (i == 0) return default;

        return RandomOrDefault(toSelectFrom);
    }

    public static T LowestOrDefault<T>(this IEnumerable<T> source, Func<T, IComparable> selector)
    {
        if (source is null) return default;

        if (!source.Any()) return default;

        var best = source.Min(v => selector(v));

        return RandomOrDefault(source.Where(v => selector(v).Equals(best)));
    }

    private static readonly Random _random = new();
    public static T RandomOrDefault<T>(this IEnumerable<T> source)
    {
        if (source is null) return default;

        var sourceArray = source.ToArray();

        if (sourceArray.Length == 0)
            return default;
        if (sourceArray.Length == 1)
            return sourceArray[0];
        return sourceArray[_random.Next(sourceArray.Length)];
    }

    public static T RandomOrDefault<T>(this IEnumerable<T> source, LoggedRandom random)
    {
        if (source is null) return default;

        var sourceArray = source.ToArray();

        if (sourceArray.Length == 0)
            return default;
        if (sourceArray.Length == 1)
            return sourceArray[0];
        return sourceArray[random.Next(sourceArray.Length)];
    }

    public static IEnumerable<T> TakeRandomN<T>(this IEnumerable<T> source, int n)
    {
        if (source is null) return Array.Empty<T>();

        return source.OrderBy(x => _random.Next()).Take(n);
    }

    public static void Set<KeyType, ValueType>(this IDictionary<KeyType, ValueType> source, KeyType key, ValueType value)
    {
        if (source.ContainsKey(key))
            source[key] = value;
        else
            source.Add(key, value);
    }
}