using System;
using System.Collections.Generic;

namespace Chummer.Backend
{
    static class IEnumerableExtensions
    {
        // Made into a not extension method as Collection.Append is confusing and no clear name could be devised
        public static IEnumerable<T> Both<T>(IEnumerable<T> first, IEnumerable<T> second)
        {
            foreach (T t in first)
            {
                yield return t;
            }

            foreach (T t in second)
            {
                yield return t;
            }
        }
    }
}
