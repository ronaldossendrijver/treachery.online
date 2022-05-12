using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class ObjectCounter<T>
    {
        private readonly Dictionary<T, int> counters = new Dictionary<T, int>();

        public void Count(T obj)
        {
            if (!counters.ContainsKey(obj))
            {
                counters.Add(obj, 1);
            }
            else
            {
                counters[obj]++;
            }
        }

        public void Count2(T obj)
        {
            if (!counters.ContainsKey(obj))
            {
                counters.Add(obj, 2);
            }
            else
            {
                counters[obj] += 2;
            }
        }

        public void CountN(T obj, int n)
        {
            if (!counters.ContainsKey(obj))
            {
                counters.Add(obj, n);
            }
            else
            {
                counters[obj] += n;
            }
        }

        public void SetToN(T obj, int n)
        {
            if (counters.ContainsKey(obj))
            {
                counters.Remove(obj);
            }

            counters.Add(obj, n);
        }

        public int CountOf(T obj)
        {
            if (counters.ContainsKey(obj))
            {
                return counters[obj];
            }

            return 0;
        }

        public IEnumerable<T> Counted => counters.Keys;

        public IEnumerable<T> GetHighest(int amountOfItems) => counters.OrderByDescending(c => c.Value).Take(amountOfItems).Select(c => c.Key);

        public T Highest
        {
            get
            {
                var bestValue = counters.Max(c => c.Value);
                var best = counters.FirstOrDefault(c => c.Value == bestValue);
                return best.Key;
            }
        }
    }
}
