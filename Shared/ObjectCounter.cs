using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class ObjectCounter<T>
    {
        private readonly Dictionary<T, int> counters = new Dictionary<T, int>();

        public void Count(T obj)
        {
            lock (counters)
            {
                if (counters.ContainsKey(obj))
                {
                    counters[obj]++;
                }
                else
                {
                    counters.Add(obj, 1);
                }
            }
        }

        public void Count2(T obj)
        {
            lock (counters)
            {
                if (counters.ContainsKey(obj))
                {
                    counters[obj] += 2;
                }
                else
                {
                    counters.Add(obj, 2);
                }
            }
        }

        public void CountN(T obj, int n)
        {
            lock (counters)
            {
                if (counters.ContainsKey(obj))
                {
                    counters[obj] += n;
                }
                else
                {
                    counters.Add(obj, n);
                }
            }
        }

        public void SetToN(T obj, int n)
        {
            lock (counters)
            {
                counters.Remove(obj);
                counters.Add(obj, n);
            }
        }

        public int CountOf(T obj)
        {
            if (counters.TryGetValue(obj, out int value))
            {
                return value;
            }

            return 0;
        }

        public IEnumerable<T> Counted => counters.Keys;

        public IEnumerable<T> GetHighest(int amountOfItems) => counters.OrderByDescending(c => c.Value).Take(amountOfItems).Select(c => c.Key);

        public T Highest
        {
            get
            {
                if (counters.Count == 0) return default;

                var bestValue = counters.Max(c => c.Value);
                var best = counters.FirstOrDefault(c => c.Value == bestValue);
                return best.Key;
            }
        }
    }
}
