using System.Collections.Generic;

namespace Chummer.Backend
{
    public static class HashSetExtensions
    {
        public static void Toggle<T>(this HashSet<T> set, T value)
        {
            if (set.Contains(value))
            {
                set.Remove(value);
            }
            else
            {
                set.Add(value);
            }
        }
    }
}