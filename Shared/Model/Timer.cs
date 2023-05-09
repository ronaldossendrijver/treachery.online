/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;

namespace Treachery.Shared
{
    public class Timer<T>
    {
        private readonly Dictionary<T, TimeSpan> _times = new();

        public TimeSpan TimeSpent(T timedItem)
        {
            if (_times.TryGetValue(timedItem, out TimeSpan ts))
            {
                return ts;
            }
            else
            {
                return TimeSpan.Zero;
            }
        }

        public void Add(T timedItem, TimeSpan ts)
        {
            if (_times.ContainsKey(timedItem))
            {
                _times[timedItem] += ts;
            }
            else
            {
                _times.Add(timedItem, ts);
            }
        }
    }
}
