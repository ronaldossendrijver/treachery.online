namespace Treachery.Shared;

public class ObjectCounter<T> where T : notnull
{
    private readonly Dictionary<T, int> _counters = new();

    public void Count(T obj)
    {
        lock (_counters)
        {
            if (!_counters.TryAdd(obj, 1))
                _counters[obj]++;
        }
    }

    public void Count2(T obj)
    {
        lock (_counters)
        {
            if (!_counters.TryAdd(obj, 2))
                _counters[obj] += 2;
        }
    }

    public void CountN(T obj, int n)
    {
        lock (_counters)
        {
            if (!_counters.TryAdd(obj, n))
                _counters[obj] += n;
        }
    }

    public void SetToN(T obj, int n)
    {
        lock (_counters)
        {
            _counters[obj] = n;
        }
    }

    public int CountOf(T obj)
    {
        lock (_counters)
        {
            return _counters.GetValueOrDefault(obj, 0);
        }
    }

    public IEnumerable<T> Counted
    {
        get { lock (_counters) { return _counters.Keys.ToList(); } }
    }

    public IEnumerable<T> GetHighest(int amountOfItems)
    {
        lock (_counters)
        {
            return _counters.OrderByDescending(c => c.Value).Take(amountOfItems).Select(c => c.Key);
        }
    }

    public T? Highest
    {
        get
        {
            lock (_counters)
            {
                if (_counters.Count == 0) return default;

                var bestValue = _counters.Max(c => c.Value);
                var best = _counters.FirstOrDefault(c => c.Value == bestValue);
                return best.Key;   
            }
        }
    }
}