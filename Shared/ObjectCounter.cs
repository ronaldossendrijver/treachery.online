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
                counters.Add(obj, 0);
            }

            counters[obj]++;
        }

        public void Count2(T obj)
        {
            if (!counters.ContainsKey(obj))
            {
                counters.Add(obj, 0);
            }

            counters[obj] += 2;
        }

        public int CountOf(T obj)
        {
            if (counters.ContainsKey(obj))
            {
                return counters[obj];
            }

            return 0;
        }

        public IEnumerable<T> Counted
        {
            get
            {
                return counters.Keys;
            }
        }

        public T Highest
        {
            get
            {
                var best = counters.OrderByDescending(c => c.Value).FirstOrDefault();
                return best.Key;
            }
        }
    }
}
