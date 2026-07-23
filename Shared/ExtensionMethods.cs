namespace Treachery.Shared;

public static class ExtensionMethods
{
    extension<T>(IEnumerable<T>? source)
    {
        public T? HighestOrDefault(Func<T, IComparable> selector)
        {
            if (source is null) return default;

            var sourceArray = source.ToArray();

            if (sourceArray.Length == 0) return default;

            var best = sourceArray.Max(selector);

            return sourceArray.Where(v => selector(v).Equals(best)).RandomOrDefault();
        }

        public T? OneOfHighestNOrDefault(Func<T, IComparable> selector, int n)
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

            return i == 0 
                ? default 
                : toSelectFrom.RandomOrDefault();
        }

        public T? OneOfLowestNOrDefault(Func<T, IComparable> selector, int n)
        {
            if (source is null || n <= 0) return default;

            List<T> toSelectFrom = [];
            var i = 0;
            foreach (var item in source.OrderBy(selector))
            {
                i++;
                toSelectFrom.Add(item);
                if (i == n) break;
            }

            return i == 0 
                ? default 
                : toSelectFrom.RandomOrDefault();
        }

        public T? LowestOrDefault(Func<T, IComparable> selector)
        {
            if (source is null) return default;
            
            var sourceArray = source.ToArray();

            if (sourceArray.Length == 0) return default;

            var best = sourceArray.Min(selector);

            return sourceArray.Where(v => selector(v).Equals(best)).RandomOrDefault();
        }
    }

    private static readonly Random Random = new();
    extension<T>(IEnumerable<T>? source)
    {
        public T? RandomOrDefault()
        {
            if (source is null) return default;

            var sourceArray = source.ToArray();

            return sourceArray.Length switch
            {
                0 => default,
                1 => sourceArray[0],
                _ => sourceArray[Random.Next(sourceArray.Length)]
            };
        }

        public T? RandomOrDefault(LoggedRandom random)
        {
            if (source is null) return default;

            var sourceArray = source.ToArray();

            return sourceArray.Length switch
            {
                0 => default,
                1 => sourceArray[0],
                _ => sourceArray[random.Next(sourceArray.Length)]
            };
        }
    }

    public static IEnumerable<T> TakeRandomN<T>(this IEnumerable<T>? source, int n)
    {
        return source is null 
            ? [] 
            : source.OrderBy(_ => Random.Next()).Take(n);
    }
}