using System;
using System.Collections.Generic;

namespace Treachery.Shared
{
    public static class Utilities
    {
        public static IList<T> CloneList<T>(IList<T> toClone) where T : ICloneable
        {
            var result = new List<T>();

            foreach (var item in toClone)
            {
                result.Add((T)item.Clone());
            }

            return result;
        }

        public static IDictionary<X, Y> CloneDictionary<X, Y>(IDictionary<X, Y> toClone) where Y : ICloneable
        {
            var result = new Dictionary<X, Y>();

            foreach (var item in toClone)
            {
                result.Add(item.Key, (Y)item.Value.Clone());
            }

            return result;
        }
    }
}
