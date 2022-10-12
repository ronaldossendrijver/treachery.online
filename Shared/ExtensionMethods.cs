using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public static class ExtensionMethods
    {
        public static T HighestOrDefault<T>(this IEnumerable<T> source, Func<T, IComparable> selector)
        {
            if (source is null)
            {
                return default;
            }

            if (!source.Any())
            {
                return default;
            }

            var best = source.Max(v => selector(v));

            return RandomOrDefault(source.Where(v => selector(v).Equals(best)));
        }

        public static T OneOfHighestNOrDefault<T>(this IEnumerable<T> source, Func<T, IComparable> selector, int n)
        {
            if (source is null || n <= 0)
            {
                return default;
            }

            List<T> toSelectFrom = new List<T>();
            int i = 0;
            foreach (var item in source.OrderByDescending(selector))
            {
                i++;
                toSelectFrom.Add(item);
                if (i == n) break;
            }

            if (i == 0)
            {
                return default;
            }

            return RandomOrDefault(toSelectFrom);
        }

        public static T OneOfLowestNOrDefault<T>(this IEnumerable<T> source, Func<T, IComparable> selector, int n)
        {
            if (source is null || n <= 0)
            {
                return default;
            }

            List<T> toSelectFrom = new List<T>();
            int i = 0;
            foreach (var item in source.OrderBy(selector))
            {
                i++;
                toSelectFrom.Add(item);
                if (i == n) break;
            }

            if (i == 0)
            {
                return default;
            }

            return RandomOrDefault(toSelectFrom);
        }

        public static T LowestOrDefault<T>(this IEnumerable<T> source, Func<T, IComparable> selector)
        {
            if (source is null)
            {
                return default;
            }

            if (!source.Any())
            {
                return default;
            }

            var best = source.Min(v => selector(v));

            return RandomOrDefault(source.Where(v => selector(v).Equals(best)));
        }

        private static readonly Random _random = new Random();
        public static T RandomOrDefault<T>(this IEnumerable<T> source)
        {
            if (source is null)
            {
                return default;
            }

            var sourceArray = source.ToArray();

            if (sourceArray.Length == 0)
            {
                return default;
            }
            else if (sourceArray.Length == 1)
            {
                return sourceArray[0];
            }
            else
            {
                return sourceArray[_random.Next(sourceArray.Length)];
            }
        }

        public static IEnumerable<T> TakeRandomN<T>(this IEnumerable<T> source, int n)
        {
            if (source is null)
            {
                return Array.Empty<T>();
            }

            return source.OrderBy(x => _random.Next()).Take(n);
        }
    }
}
