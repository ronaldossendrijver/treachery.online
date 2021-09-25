using System;
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

        public static T RandomOrDefault<T>(this IEnumerable<T> source)
        {
            if (source is null)
            {
                return default;
            }

            if (!source.Any())
            {
                return default;
            }

            var toTake = (new Random()).Next(source.Count());

            try
            {
                return source.ElementAt(toTake);
            }
            catch (Exception)
            {
                Console.WriteLine("items: {0}, toTake: {1}", source.Count(), toTake);
            }

            return default;
        }
    }
}
