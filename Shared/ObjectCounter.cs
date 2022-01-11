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
