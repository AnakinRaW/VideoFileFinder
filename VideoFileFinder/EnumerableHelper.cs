using System.Collections.Generic;
using System.Linq;

namespace VideoFileFinder
{
    public static class EnumerableHelper
    {
        public static bool ContainsAllItems<T>(this IEnumerable<T> a, IEnumerable<T> b)
        {
            return !b.Except(a).Any();
        }
    }
}