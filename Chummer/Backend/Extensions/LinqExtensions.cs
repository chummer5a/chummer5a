/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Chummer.Annotations;

namespace Chummer
{
    public static class LinqExtensions
    {
        /// <summary>
        /// Similar to LINQ's Aggregate(), but deep searches the list, applying the aggregator to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static TSource DeepAggregate<TSource>(this IEnumerable<TSource> objParentList, Func<TSource, IEnumerable<TSource>> funcGetChildrenMethod, Func<TSource, TSource, TSource> funcAggregate)
        {
            return objParentList == null
                ? default
                : objParentList.Aggregate<TSource, TSource>(default,
                    (current, objLoopChild) => funcAggregate(funcAggregate(current, objLoopChild),
                        funcGetChildrenMethod(objLoopChild).DeepAggregate(funcGetChildrenMethod, funcAggregate)));
        }

        /// <summary>
        /// Similar to LINQ's Aggregate(), but deep searches the list, applying the aggregator to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static TAccumulate DeepAggregate<TSource, TAccumulate>(this IEnumerable<TSource> objParentList, Func<TSource, IEnumerable<TSource>> funcGetChildrenMethod, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> funcAggregate)
        {
            return objParentList == null
                ? seed
                : objParentList.Aggregate(seed,
                    (current, objLoopChild) => funcGetChildrenMethod(objLoopChild).DeepAggregate(funcGetChildrenMethod,
                        funcAggregate(current, objLoopChild), funcAggregate));
        }

        /// <summary>
        /// Similar to LINQ's Aggregate(), but deep searches the list, applying the aggregator to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static TResult DeepAggregate<TSource, TAccumulate, TResult>(this IEnumerable<TSource> objParentList, Func<TSource, IEnumerable<TSource>> funcGetChildrenMethod, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> funcAggregate, Func<TAccumulate, TResult> resultSelector)
        {
            return resultSelector == null
                ? default
                : resultSelector(objParentList.DeepAggregate(funcGetChildrenMethod, seed, funcAggregate));
        }

        /// <summary>
        /// Syntactic sugar for a version of SequenceEquals that does not care about the order of elements, just that 
        /// </summary>
        /// <param name="first">First collection to compare.</param>
        /// <param name="second">Second collection to compare.</param>
        /// <returns>True if <paramref name="first"/> and <paramref name="second"/> are of the same size and have the same contents, false otherwise.</returns>
        public static bool CollectionEqual<T>([NotNull] this IReadOnlyCollection<T> first, [NotNull] IReadOnlyCollection<T> second)
        {
            if (first.Count != second.Count)
                return false;
            // Use built-in, faster implementations if they are available
            if (first is ISet<T> setFirst)
                return setFirst.SetEquals(second);
            if (second is ISet<T> setSecond)
                return setSecond.SetEquals(first);
            return first.Concat(second).Distinct().All(objItem => first.Count(x => x.Equals(objItem)) == second.Count(x => x.Equals(objItem)));
        }

        /// <summary>
        /// Syntactic sugar for a version of SequenceEquals that does not care about the order of elements, just that 
        /// </summary>
        /// <param name="first">First collection to compare.</param>
        /// <param name="second">Second collection to compare.</param>
        /// <param name="comparer">Special equality comparer to use instead of the default one.</param>
        /// <returns>True if <paramref name="first"/> and <paramref name="second"/> are of the same size and have the same contents, false otherwise.</returns>
        public static bool CollectionEqual<T>([NotNull] this IReadOnlyCollection<T> first, [NotNull] IReadOnlyCollection<T> second, IEqualityComparer<T> comparer)
        {
            // Sets do not have IEqualityComparer versions for SetEquals, so we always need to do this the slow way
            return first.Count == second.Count && first.Concat(second).Distinct().All(objItem =>
                first.Count(x => comparer.Equals(x, objItem)) == second.Count(x => comparer.Equals(x, objItem)));
        }

        /// <summary>
        /// Deep searches two collections to make sure matching elements between the two fulfill some predicate. Each item in the target list can only be matched once.
        /// </summary>
        /// <param name="objParentList">Base list to check.</param>
        /// <param name="objTargetList">Target list against which we're checking</param>
        /// <param name="funcGetChildrenMethod">Method used to get children of both the base list and the target list against which we're checking.</param>
        /// <param name="predicate">Two-argument function that takes its first argument from the base list and the second from the target list. If it does not return true on any available pair, the method returns false.</param>
        public static bool DeepMatch<T>(this IReadOnlyCollection<T> objParentList, IReadOnlyCollection<T> objTargetList, Func<T, IReadOnlyCollection<T>> funcGetChildrenMethod, Func<T, T, bool> predicate)
        {
            if (objParentList == null || objTargetList == null)
                return objParentList == null && objTargetList == null;
            if (objParentList.Count != objTargetList.Count)
                return false;
            HashSet<T> lstExclude = new HashSet<T>();
            foreach (T objLoopChild in objParentList)
            {
                foreach (T objTargetChild in objTargetList)
                {
                    if (!lstExclude.Contains(objTargetChild)
                        && predicate(objLoopChild, objTargetChild)
                        && funcGetChildrenMethod(objLoopChild).DeepMatch(funcGetChildrenMethod(objTargetChild), funcGetChildrenMethod, predicate))
                    {
                        lstExclude.Add(objTargetChild);
                        goto NextItem;
                    }
                }
                // No matching item was found, return false
                return false;
            NextItem:;
            }
            return true;
        }

        /// <summary>
        /// Similar to LINQ's All(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static bool DeepAll<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            return objParentList.All(objLoopChild =>
                predicate(objLoopChild) &&
                funcGetChildrenMethod(objLoopChild).DeepAll(funcGetChildrenMethod, predicate));
        }

        /// <summary>
        /// Similar to LINQ's Any(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static bool DeepAny<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            return objParentList.Any(objLoopChild =>
                predicate(objLoopChild) ||
                funcGetChildrenMethod(objLoopChild).DeepAny(funcGetChildrenMethod, predicate));
        }

        /// <summary>
        /// Similar to LINQ's Count(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static int DeepCount<T>(this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            if (objParentList == null)
                return 0;
            int intReturn = 0;
            foreach (T objLoopChild in objParentList)
            {
                if (predicate(objLoopChild))
                    intReturn += 1;
                intReturn += funcGetChildrenMethod(objLoopChild).DeepCount(funcGetChildrenMethod, predicate);
            }
            return intReturn;
        }

        /// <summary>
        /// Similar to LINQ's Count() without predicate, but deep searches the list, counting up the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static int DeepCount<T>(this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod)
        {
            return objParentList?.Sum(objLoopChild =>
                1 + funcGetChildrenMethod(objLoopChild).DeepCount(funcGetChildrenMethod)) ?? 0;
        }

        /// <summary>
        /// Similar to LINQ's First(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static T DeepFirst<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            foreach (T objLoopChild in objParentList)
            {
                if (predicate(objLoopChild))
                    return objLoopChild;
                T objReturn = funcGetChildrenMethod(objLoopChild).DeepFirstOrDefault(funcGetChildrenMethod, predicate);
                if (objReturn?.Equals(default(T)) == false)
                    return objReturn;
            }
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to LINQ's FirstOrDefault(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static T DeepFirstOrDefault<T>(this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            if (objParentList == null)
                return default;
            foreach (T objLoopChild in objParentList)
            {
                if (predicate(objLoopChild))
                    return objLoopChild;
                T objReturn = funcGetChildrenMethod(objLoopChild).DeepFirstOrDefault(funcGetChildrenMethod, predicate);
                if (objReturn?.Equals(default(T)) == false)
                    return objReturn;
            }
            return default;
        }

        /// <summary>
        /// Similar to LINQ's Last(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static T DeepLast<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            T objReturn = objParentList.DeepLastOrDefault(funcGetChildrenMethod, predicate);
            if (objReturn?.Equals(default(T)) == false)
                return objReturn;
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to LINQ's Last() without a predicate, but deep searches the list, returning the last element out of the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static T DeepLast<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod)
        {
            T objReturn = objParentList.DeepLastOrDefault(funcGetChildrenMethod);
            if (objReturn?.Equals(default(T)) == false)
                return objReturn;
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to LINQ's LastOrDefault(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static T DeepLastOrDefault<T>(this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            if (objParentList == null)
                return default;
            T objReturn = default;
            foreach (T objLoopChild in objParentList)
            {
                if (predicate(objLoopChild))
                    objReturn = objLoopChild;
                T objTemp = funcGetChildrenMethod(objLoopChild).DeepLastOrDefault(funcGetChildrenMethod, predicate);
                if (objTemp?.Equals(default(T)) == false)
                    objReturn = objTemp;
            }
            return objReturn;
        }

        /// <summary>
        /// Similar to LINQ's LastOrDefault() without a predicate, but deep searches the list, returning the last element out of the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static T DeepLastOrDefault<T>(this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod)
        {
            if (objParentList == null)
                return default;
            T objReturn = objParentList.LastOrDefault();
            if (funcGetChildrenMethod != null)
            {
                List<T> lstChildren = funcGetChildrenMethod(objReturn).ToList();
                if (lstChildren.Count > 0)
                {
                    T objTemp = lstChildren.DeepLastOrDefault(funcGetChildrenMethod);
                    if (objTemp?.Equals(default(T)) == false)
                        return objTemp;
                }
            }
            return objReturn;
        }

        /// <summary>
        /// Similar to LINQ's Where(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static IEnumerable<T> DeepWhere<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            foreach (T objLoopChild in objParentList)
            {
                if (predicate(objLoopChild))
                    yield return objLoopChild;

                foreach (T objLoopSubchild in funcGetChildrenMethod(objLoopChild).DeepWhere(funcGetChildrenMethod, predicate))
                    yield return objLoopSubchild;
            }
        }

        /// <summary>
        /// Gets all relatives in the list, including the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static IEnumerable<T> GetAllDescendants<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod)
        {
            foreach (T objLoopChild in objParentList)
            {
                yield return objLoopChild;

                foreach (T objLoopSubchild in funcGetChildrenMethod(objLoopChild).GetAllDescendants(funcGetChildrenMethod))
                    yield return objLoopSubchild;
            }
        }
    }
}
