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
using System.Threading;
using System.Threading.Tasks;
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
        /// Syntactic sugar for a version of SequenceEquals that does not care about the order of elements, just the two collections' contents.
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
            if (first.GetOrderInvariantEnsembleHashCodeSmart() != second.GetOrderInvariantEnsembleHashCodeSmart())
                return false;
            List<T> lstTemp = second.ToList();
            foreach (T item in first)
            {
                if (!lstTemp.Remove(item))
                    return false;
            }
            return lstTemp.Count == 0; // The list will only be empty if all elements in second are also in first
        }

        /// <summary>
        /// Syntactic sugar for a version of SequenceEquals that does not care about the order of elements, just the two collections' contents.
        /// </summary>
        /// <param name="first">First collection to compare.</param>
        /// <param name="second">Second collection to compare.</param>
        /// <param name="comparer">Special equality comparer to use instead of the default one.</param>
        /// <returns>True if <paramref name="first"/> and <paramref name="second"/> are of the same size and have the same contents, false otherwise.</returns>
        public static bool CollectionEqual<T>([NotNull] this IReadOnlyCollection<T> first, [NotNull] IReadOnlyCollection<T> second, IEqualityComparer<T> comparer)
        {
            // Sets do not have IEqualityComparer versions for SetEquals, so we always need to do this the slow way
            if (first.Count != second.Count)
                return false;
            // Cannot use hashes because the equality comparer might not be compatible with them (it could mark two objects with different hashes equal)
            List<T> lstTemp = second.ToList();
            foreach (T item in first)
            {
                int i = lstTemp.FindIndex(x => comparer.Equals(x, item));
                if (i < 0)
                    return false;
                lstTemp.RemoveAt(i);
            }
            return lstTemp.Count == 0; // The list will only be empty if all elements in second are also in first
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
            HashSet<T> setExclude = new HashSet<T>();
            foreach (T objLoopChild in objParentList)
            {
                bool blnFoundItem = false;
                foreach (T objTargetChild in objTargetList)
                {
                    if (!setExclude.Contains(objTargetChild)
                        && predicate(objLoopChild, objTargetChild)
                        && funcGetChildrenMethod(objLoopChild).DeepMatch(funcGetChildrenMethod(objTargetChild), funcGetChildrenMethod, predicate))
                    {
                        setExclude.Add(objTargetChild);
                        blnFoundItem = true;
                        break;
                    }
                }
                // No matching item was found, return false
                if (!blnFoundItem)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Deep searches two collections to make sure matching elements between the two fulfill some predicate. Each item in the target list can only be matched once.
        /// </summary>
        /// <param name="objParentList">Base list to check.</param>
        /// <param name="objTargetList">Target list against which we're checking</param>
        /// <param name="funcGetChildrenMethod">Method used to get children of both the base list and the target list against which we're checking.</param>
        /// <param name="predicate">Two-argument function that takes its first argument from the base list and the second from the target list. If it does not return true on any available pair, the method returns false.</param>
        public static bool DeepMatch(this IReadOnlyCollection<string> objParentList, IReadOnlyCollection<string> objTargetList, Func<string, IReadOnlyCollection<string>> funcGetChildrenMethod, Func<string, string, bool> predicate)
        {
            if (objParentList == null || objTargetList == null)
                return objParentList == null && objTargetList == null;
            if (objParentList.Count != objTargetList.Count)
                return false;
            using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool, out HashSet<string> setExclude))
            {
                foreach (string objLoopChild in objParentList)
                {
                    bool blnFoundItem = false;
                    foreach (string objTargetChild in objTargetList)
                    {
                        if (!setExclude.Contains(objTargetChild)
                            && predicate(objLoopChild, objTargetChild)
                            && funcGetChildrenMethod(objLoopChild)
                                .DeepMatch(funcGetChildrenMethod(objTargetChild), funcGetChildrenMethod, predicate))
                        {
                            setExclude.Add(objTargetChild);
                            blnFoundItem = true;
                            break;
                        }
                    }

                    // No matching item was found, return false
                    if (!blnFoundItem)
                        return false;
                }
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
        /// Similar to LINQ's All(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<bool> DeepAll<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (!predicate(objLoopChild) || !await (await funcGetChildrenMethod(objLoopChild)).DeepAll(funcGetChildrenMethod, predicate, token))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Similar to LINQ's All(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<bool> DeepAll<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, CancellationToken, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (!predicate(objLoopChild) || !await (await funcGetChildrenMethod(objLoopChild, token)).DeepAll(funcGetChildrenMethod, predicate, token))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Similar to LINQ's All(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<bool> DeepAll<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (!await predicate(objLoopChild) || !await funcGetChildrenMethod(objLoopChild).DeepAll(funcGetChildrenMethod, predicate, token))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Similar to LINQ's All(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<bool> DeepAll<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, CancellationToken, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (!await predicate(objLoopChild, token) || !await funcGetChildrenMethod(objLoopChild).DeepAll(funcGetChildrenMethod, predicate, token))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Similar to LINQ's All(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<bool> DeepAll<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (!await predicate(objLoopChild) || !await (await funcGetChildrenMethod(objLoopChild)).DeepAll(funcGetChildrenMethod, predicate, token))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Similar to LINQ's All(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<bool> DeepAll<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, CancellationToken, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (!await predicate(objLoopChild, token) || !await (await funcGetChildrenMethod(objLoopChild)).DeepAll(funcGetChildrenMethod, predicate, token))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Similar to LINQ's All(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<bool> DeepAll<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, CancellationToken, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (!await predicate(objLoopChild) || !await (await funcGetChildrenMethod(objLoopChild, token)).DeepAll(funcGetChildrenMethod, predicate, token))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Similar to LINQ's All(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<bool> DeepAll<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, CancellationToken, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, CancellationToken, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (!await predicate(objLoopChild, token) || !await (await funcGetChildrenMethod(objLoopChild, token)).DeepAll(funcGetChildrenMethod, predicate, token))
                    return false;
            }

            return true;
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
        /// Similar to LINQ's Any(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<bool> DeepAny<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (predicate(objLoopChild) || await (await funcGetChildrenMethod(objLoopChild)).DeepAny(funcGetChildrenMethod, predicate, token))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Similar to LINQ's Any(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<bool> DeepAny<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, CancellationToken, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (predicate(objLoopChild) || await (await funcGetChildrenMethod(objLoopChild, token)).DeepAny(funcGetChildrenMethod, predicate, token))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Similar to LINQ's Any(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<bool> DeepAny<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (await predicate(objLoopChild) || await funcGetChildrenMethod(objLoopChild).DeepAny(funcGetChildrenMethod, predicate, token))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Similar to LINQ's Any(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<bool> DeepAny<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, CancellationToken, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (await predicate(objLoopChild, token) || await funcGetChildrenMethod(objLoopChild).DeepAny(funcGetChildrenMethod, predicate, token))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Similar to LINQ's Any(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<bool> DeepAny<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (await predicate(objLoopChild) || await (await funcGetChildrenMethod(objLoopChild)).DeepAny(funcGetChildrenMethod, predicate, token))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Similar to LINQ's Any(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<bool> DeepAny<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, CancellationToken, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (T objLoopChild in objParentList)
            {
                if (await predicate(objLoopChild, token) || await (await funcGetChildrenMethod(objLoopChild)).DeepAny(funcGetChildrenMethod, predicate, token))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Similar to LINQ's Any(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<bool> DeepAny<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, CancellationToken, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (await predicate(objLoopChild) || await (await funcGetChildrenMethod(objLoopChild, token)).DeepAny(funcGetChildrenMethod, predicate, token))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Similar to LINQ's Any(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<bool> DeepAny<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, CancellationToken, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, CancellationToken, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (await predicate(objLoopChild, token) || await (await funcGetChildrenMethod(objLoopChild, token)).DeepAny(funcGetChildrenMethod, predicate, token))
                    return true;
            }

            return false;
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
                    ++intReturn;
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
        /// Similar to LINQ's First(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepFirst<T>(this IEnumerable<T> objParentList, Func<T, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (predicate(objLoopChild))
                    return objLoopChild;
                token.ThrowIfCancellationRequested();
                T objReturn = await (await funcGetChildrenMethod(objLoopChild)).DeepFirst(funcGetChildrenMethod, predicate, token);
                token.ThrowIfCancellationRequested();
                if (objReturn?.Equals(default(T)) == false)
                    return objReturn;
            }
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to LINQ's First(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepFirst<T>(this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (await predicate(objLoopChild))
                    return objLoopChild;
                token.ThrowIfCancellationRequested();
                T objReturn = await funcGetChildrenMethod(objLoopChild).DeepFirst(funcGetChildrenMethod, predicate, token);
                token.ThrowIfCancellationRequested();
                if (objReturn?.Equals(default(T)) == false)
                    return objReturn;
            }
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to LINQ's First(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepFirst<T>(this IEnumerable<T> objParentList, Func<T, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (await predicate(objLoopChild))
                    return objLoopChild;
                token.ThrowIfCancellationRequested();
                T objReturn = await (await funcGetChildrenMethod(objLoopChild)).DeepFirst(funcGetChildrenMethod, predicate, token);
                token.ThrowIfCancellationRequested();
                if (objReturn?.Equals(default(T)) == false)
                    return objReturn;
            }
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to LINQ's First(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepFirst<T>(this IEnumerable<T> objParentList, Func<T, CancellationToken, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (predicate(objLoopChild))
                    return objLoopChild;
                token.ThrowIfCancellationRequested();
                T objReturn = await (await funcGetChildrenMethod(objLoopChild, token)).DeepFirst(funcGetChildrenMethod, predicate, token);
                if (objReturn?.Equals(default(T)) == false)
                    return objReturn;
            }
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to LINQ's First(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepFirst<T>(this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, CancellationToken, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (await predicate(objLoopChild, token))
                    return objLoopChild;
                token.ThrowIfCancellationRequested();
                T objReturn = await funcGetChildrenMethod(objLoopChild).DeepFirst(funcGetChildrenMethod, predicate, token);
                if (objReturn?.Equals(default(T)) == false)
                    return objReturn;
            }
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to LINQ's First(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepFirst<T>(this IEnumerable<T> objParentList, Func<T, CancellationToken, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, CancellationToken, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (await predicate(objLoopChild, token))
                    return objLoopChild;
                token.ThrowIfCancellationRequested();
                T objReturn = await (await funcGetChildrenMethod(objLoopChild, token)).DeepFirst(funcGetChildrenMethod, predicate, token);
                if (objReturn?.Equals(default(T)) == false)
                    return objReturn;
            }
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to LINQ's FirstOrDefault(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static T DeepFirstOrDefault<T>(this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (predicate(objLoopChild))
                    return objLoopChild;
                token.ThrowIfCancellationRequested();
                T objReturn = funcGetChildrenMethod(objLoopChild).DeepFirstOrDefault(funcGetChildrenMethod, predicate, token);
                token.ThrowIfCancellationRequested();
                if (objReturn?.Equals(default(T)) == false)
                    return objReturn;
            }
            return default;
        }

        /// <summary>
        /// Similar to LINQ's FirstOrDefault(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepFirstOrDefault<T>(this IEnumerable<T> objParentList, Func<T, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (predicate(objLoopChild))
                    return objLoopChild;
                token.ThrowIfCancellationRequested();
                T objReturn = await (await funcGetChildrenMethod(objLoopChild)).DeepFirstOrDefault(funcGetChildrenMethod, predicate, token);
                token.ThrowIfCancellationRequested();
                if (objReturn?.Equals(default(T)) == false)
                    return objReturn;
            }
            return default;
        }

        /// <summary>
        /// Similar to LINQ's FirstOrDefault(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepFirstOrDefault<T>(this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (await predicate(objLoopChild))
                    return objLoopChild;
                token.ThrowIfCancellationRequested();
                T objReturn = await funcGetChildrenMethod(objLoopChild).DeepFirstOrDefault(funcGetChildrenMethod, predicate, token);
                token.ThrowIfCancellationRequested();
                if (objReturn?.Equals(default(T)) == false)
                    return objReturn;
            }
            return default;
        }

        /// <summary>
        /// Similar to LINQ's FirstOrDefault(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepFirstOrDefault<T>(this IEnumerable<T> objParentList, Func<T, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (await predicate(objLoopChild))
                    return objLoopChild;
                token.ThrowIfCancellationRequested();
                T objReturn = await (await funcGetChildrenMethod(objLoopChild)).DeepFirstOrDefault(funcGetChildrenMethod, predicate, token);
                token.ThrowIfCancellationRequested();
                if (objReturn?.Equals(default(T)) == false)
                    return objReturn;
            }
            return default;
        }

        /// <summary>
        /// Similar to LINQ's FirstOrDefault(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepFirstOrDefault<T>(this IEnumerable<T> objParentList, Func<T, CancellationToken, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (predicate(objLoopChild))
                    return objLoopChild;
                token.ThrowIfCancellationRequested();
                T objReturn = await (await funcGetChildrenMethod(objLoopChild, token)).DeepFirstOrDefault(funcGetChildrenMethod, predicate, token);
                if (objReturn?.Equals(default(T)) == false)
                    return objReturn;
            }
            return default;
        }

        /// <summary>
        /// Similar to LINQ's FirstOrDefault(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepFirstOrDefault<T>(this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, CancellationToken, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (await predicate(objLoopChild, token))
                    return objLoopChild;
                token.ThrowIfCancellationRequested();
                T objReturn = await funcGetChildrenMethod(objLoopChild).DeepFirstOrDefault(funcGetChildrenMethod, predicate, token);
                if (objReturn?.Equals(default(T)) == false)
                    return objReturn;
            }
            return default;
        }

        /// <summary>
        /// Similar to LINQ's FirstOrDefault(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepFirstOrDefault<T>(this IEnumerable<T> objParentList, Func<T, CancellationToken, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, CancellationToken, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (await predicate(objLoopChild, token))
                    return objLoopChild;
                token.ThrowIfCancellationRequested();
                T objReturn = await (await funcGetChildrenMethod(objLoopChild, token)).DeepFirstOrDefault(funcGetChildrenMethod, predicate, token);
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
        /// Similar to LINQ's Last(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLast<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            T objReturn = await objParentList.DeepLastOrDefault(funcGetChildrenMethod, predicate, token);
            token.ThrowIfCancellationRequested();
            if (objReturn?.Equals(default(T)) == false)
                return objReturn;
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to LINQ's Last(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLast<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            T objReturn = await objParentList.DeepLastOrDefault(funcGetChildrenMethod, predicate, token);
            token.ThrowIfCancellationRequested();
            if (objReturn?.Equals(default(T)) == false)
                return objReturn;
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to LINQ's Last(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLast<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            T objReturn = await objParentList.DeepLastOrDefault(funcGetChildrenMethod, predicate, token);
            token.ThrowIfCancellationRequested();
            if (objReturn?.Equals(default(T)) == false)
                return objReturn;
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to LINQ's Last(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLast<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, CancellationToken, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            T objReturn = await objParentList.DeepLastOrDefault(funcGetChildrenMethod, predicate, token);
            token.ThrowIfCancellationRequested();
            if (objReturn?.Equals(default(T)) == false)
                return objReturn;
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to LINQ's Last(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLast<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, CancellationToken, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            T objReturn = await objParentList.DeepLastOrDefault(funcGetChildrenMethod, predicate, token);
            token.ThrowIfCancellationRequested();
            if (objReturn?.Equals(default(T)) == false)
                return objReturn;
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to LINQ's Last(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLast<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, CancellationToken, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, CancellationToken, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            T objReturn = await objParentList.DeepLastOrDefault(funcGetChildrenMethod, predicate, token);
            token.ThrowIfCancellationRequested();
            if (objReturn?.Equals(default(T)) == false)
                return objReturn;
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to LINQ's Last() without a predicate, but deep searches the list, returning the last element out of the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLast<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, Task<IEnumerable<T>>> funcGetChildrenMethod, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            T objReturn = await objParentList.DeepLastOrDefault(funcGetChildrenMethod, token);
            token.ThrowIfCancellationRequested();
            if (objReturn?.Equals(default(T)) == false)
                return objReturn;
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to LINQ's Last() without a predicate, but deep searches the list, returning the last element out of the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLast<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, CancellationToken, Task<IEnumerable<T>>> funcGetChildrenMethod, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            T objReturn = await objParentList.DeepLastOrDefault(funcGetChildrenMethod, token);
            token.ThrowIfCancellationRequested();
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
        /// Similar to LINQ's LastOrDefault(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastOrDefault<T>(this IEnumerable<T> objParentList, Func<T, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            T objReturn = default;
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (predicate(objLoopChild))
                    objReturn = objLoopChild;
                token.ThrowIfCancellationRequested();
                T objTemp = await (await funcGetChildrenMethod(objLoopChild)).DeepLastOrDefault(funcGetChildrenMethod, predicate, token);
                token.ThrowIfCancellationRequested();
                if (objTemp?.Equals(default(T)) == false)
                    objReturn = objTemp;
            }
            return objReturn;
        }

        /// <summary>
        /// Similar to LINQ's LastOrDefault(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastOrDefault<T>(this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            T objReturn = default;
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (await predicate(objLoopChild))
                    objReturn = objLoopChild;
                token.ThrowIfCancellationRequested();
                T objTemp = await funcGetChildrenMethod(objLoopChild).DeepLastOrDefault(funcGetChildrenMethod, predicate, token);
                token.ThrowIfCancellationRequested();
                if (objTemp?.Equals(default(T)) == false)
                    objReturn = objTemp;
            }
            return objReturn;
        }

        /// <summary>
        /// Similar to LINQ's LastOrDefault(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastOrDefault<T>(this IEnumerable<T> objParentList, Func<T, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            T objReturn = default;
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (await predicate(objLoopChild))
                    objReturn = objLoopChild;
                token.ThrowIfCancellationRequested();
                T objTemp = await (await funcGetChildrenMethod(objLoopChild)).DeepLastOrDefault(funcGetChildrenMethod, predicate, token);
                token.ThrowIfCancellationRequested();
                if (objTemp?.Equals(default(T)) == false)
                    objReturn = objTemp;
            }
            return objReturn;
        }

        /// <summary>
        /// Similar to LINQ's LastOrDefault(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastOrDefault<T>(this IEnumerable<T> objParentList, Func<T, CancellationToken, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            T objReturn = default;
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (predicate(objLoopChild))
                    objReturn = objLoopChild;
                token.ThrowIfCancellationRequested();
                T objTemp = await (await funcGetChildrenMethod(objLoopChild, token)).DeepLastOrDefault(funcGetChildrenMethod, predicate, token);
                token.ThrowIfCancellationRequested();
                if (objTemp?.Equals(default(T)) == false)
                    objReturn = objTemp;
            }
            return objReturn;
        }

        /// <summary>
        /// Similar to LINQ's LastOrDefault(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastOrDefault<T>(this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, CancellationToken, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            T objReturn = default;
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (await predicate(objLoopChild, token))
                    objReturn = objLoopChild;
                token.ThrowIfCancellationRequested();
                T objTemp = await funcGetChildrenMethod(objLoopChild).DeepLastOrDefault(funcGetChildrenMethod, predicate, token);
                token.ThrowIfCancellationRequested();
                if (objTemp?.Equals(default(T)) == false)
                    objReturn = objTemp;
            }
            return objReturn;
        }

        /// <summary>
        /// Similar to LINQ's LastOrDefault(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastOrDefault<T>(this IEnumerable<T> objParentList, Func<T, CancellationToken, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, CancellationToken, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            T objReturn = default;
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (await predicate(objLoopChild, token))
                    objReturn = objLoopChild;
                token.ThrowIfCancellationRequested();
                T objTemp = await (await funcGetChildrenMethod(objLoopChild, token)).DeepLastOrDefault(funcGetChildrenMethod, predicate, token);
                token.ThrowIfCancellationRequested();
                if (objTemp?.Equals(default(T)) == false)
                    objReturn = objTemp;
            }
            return objReturn;
        }

        /// <summary>
        /// Similar to LINQ's LastOrDefault() without a predicate, but deep searches the list, returning the last element out of the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastOrDefault<T>(this IEnumerable<T> objParentList, Func<T, Task<IEnumerable<T>>> funcGetChildrenMethod, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            T objReturn = objParentList.LastOrDefault();
            token.ThrowIfCancellationRequested();
            if (funcGetChildrenMethod != null)
            {
                List<T> lstChildren = (await funcGetChildrenMethod(objReturn)).ToList();
                token.ThrowIfCancellationRequested();
                if (lstChildren.Count > 0)
                {
                    T objTemp = await lstChildren.DeepLastOrDefault(funcGetChildrenMethod, token);
                    token.ThrowIfCancellationRequested();
                    if (objTemp?.Equals(default(T)) == false)
                        return objTemp;
                }
            }
            return objReturn;
        }

        /// <summary>
        /// Similar to LINQ's LastOrDefault() without a predicate, but deep searches the list, returning the last element out of the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastOrDefault<T>(this IEnumerable<T> objParentList, Func<T, CancellationToken, Task<IEnumerable<T>>> funcGetChildrenMethod, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            T objReturn = objParentList.LastOrDefault();
            token.ThrowIfCancellationRequested();
            if (funcGetChildrenMethod != null)
            {
                List<T> lstChildren = (await funcGetChildrenMethod(objReturn, token)).ToList();
                token.ThrowIfCancellationRequested();
                if (lstChildren.Count > 0)
                {
                    T objTemp = await lstChildren.DeepLastOrDefault(funcGetChildrenMethod, token);
                    token.ThrowIfCancellationRequested();
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

                foreach (T objLoopGrandchild in funcGetChildrenMethod(objLoopChild).DeepWhere(funcGetChildrenMethod, predicate))
                    yield return objLoopGrandchild;
            }
        }

        /// <summary>
        /// Similar to LINQ's Where(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<List<T>> DeepWhere<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<T> lstReturn = new List<T>();
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (predicate(objLoopChild))
                    lstReturn.Add(objLoopChild);
                token.ThrowIfCancellationRequested();
                lstReturn.AddRange(await (await funcGetChildrenMethod(objLoopChild)).DeepWhere(funcGetChildrenMethod, predicate, token));
            }

            return lstReturn;
        }

        /// <summary>
        /// Similar to LINQ's Where(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<List<T>> DeepWhere<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<T> lstReturn = new List<T>();
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (await predicate(objLoopChild))
                    lstReturn.Add(objLoopChild);
                token.ThrowIfCancellationRequested();
                lstReturn.AddRange(await funcGetChildrenMethod(objLoopChild).DeepWhere(funcGetChildrenMethod, predicate, token));
            }

            return lstReturn;
        }

        /// <summary>
        /// Similar to LINQ's Where(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<List<T>> DeepWhere<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<T> lstReturn = new List<T>();
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (await predicate(objLoopChild))
                    lstReturn.Add(objLoopChild);
                token.ThrowIfCancellationRequested();
                lstReturn.AddRange(await (await funcGetChildrenMethod(objLoopChild)).DeepWhere(funcGetChildrenMethod, predicate, token));
            }

            return lstReturn;
        }

        /// <summary>
        /// Similar to LINQ's Where(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<List<T>> DeepWhere<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, CancellationToken, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<T> lstReturn = new List<T>();
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (predicate(objLoopChild))
                    lstReturn.Add(objLoopChild);
                token.ThrowIfCancellationRequested();
                lstReturn.AddRange(await (await funcGetChildrenMethod(objLoopChild, token)).DeepWhere(funcGetChildrenMethod, predicate, token));
            }

            return lstReturn;
        }

        /// <summary>
        /// Similar to LINQ's Where(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<List<T>> DeepWhere<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, CancellationToken, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<T> lstReturn = new List<T>();
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (await predicate(objLoopChild, token))
                    lstReturn.Add(objLoopChild);
                token.ThrowIfCancellationRequested();
                lstReturn.AddRange(await funcGetChildrenMethod(objLoopChild).DeepWhere(funcGetChildrenMethod, predicate, token));
            }

            return lstReturn;
        }

        /// <summary>
        /// Similar to LINQ's Where(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<List<T>> DeepWhere<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, CancellationToken, Task<IEnumerable<T>>> funcGetChildrenMethod, Func<T, CancellationToken, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<T> lstReturn = new List<T>();
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                if (await predicate(objLoopChild, token))
                    lstReturn.Add(objLoopChild);
                token.ThrowIfCancellationRequested();
                lstReturn.AddRange(await (await funcGetChildrenMethod(objLoopChild, token)).DeepWhere(funcGetChildrenMethod, predicate, token));
            }

            return lstReturn;
        }

        /// <summary>
        /// Gets all relatives in the list, including the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static IEnumerable<T> GetAllDescendants<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod)
        {
            foreach (T objLoopChild in objParentList)
            {
                yield return objLoopChild;

                foreach (T objLoopGrandchild in funcGetChildrenMethod(objLoopChild).GetAllDescendants(funcGetChildrenMethod))
                    yield return objLoopGrandchild;
            }
        }

        /// <summary>
        /// Gets all relatives in the list, including the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<List<T>> GetAllDescendants<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, Task<IEnumerable<T>>> funcGetChildrenMethod, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<T> lstReturn = new List<T>();
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                lstReturn.Add(objLoopChild);
                token.ThrowIfCancellationRequested();
                lstReturn.AddRange(await (await funcGetChildrenMethod(objLoopChild)).GetAllDescendants(funcGetChildrenMethod));
            }
            return lstReturn;
        }

        /// <summary>
        /// Gets all relatives in the list, including the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<List<T>> GetAllDescendants<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, CancellationToken, Task<IEnumerable<T>>> funcGetChildrenMethod, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<T> lstReturn = new List<T>();
            foreach (T objLoopChild in objParentList)
            {
                token.ThrowIfCancellationRequested();
                lstReturn.Add(objLoopChild);
                token.ThrowIfCancellationRequested();
                lstReturn.AddRange(await (await funcGetChildrenMethod(objLoopChild, token)).GetAllDescendants(funcGetChildrenMethod, token));
            }
            return lstReturn;
        }

        public static int Sum<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            int intReturn = 0;
            foreach (T objCurrent in objEnumerable)
            {
                token.ThrowIfCancellationRequested();
                if (funcPredicate(objCurrent))
                    intReturn += funcSelector.Invoke(objCurrent);
            }
            return intReturn;
        }

        public static int Sum<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            int intReturn = 0;
            foreach (T objCurrent in objEnumerable)
            {
                token.ThrowIfCancellationRequested();
                if (funcPredicate(objCurrent))
                    intReturn += Utils.SafelyRunSynchronously(() => funcSelector.Invoke(objCurrent), token);
            }
            return intReturn;
        }

        public static long Sum<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            long lngReturn = 0;
            foreach (T objCurrent in objEnumerable)
            {
                token.ThrowIfCancellationRequested();
                if (funcPredicate(objCurrent))
                    lngReturn += funcSelector.Invoke(objCurrent);
            }
            return lngReturn;
        }

        public static long Sum<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            long lngReturn = 0;
            foreach (T objCurrent in objEnumerable)
            {
                token.ThrowIfCancellationRequested();
                if (funcPredicate(objCurrent))
                    lngReturn += Utils.SafelyRunSynchronously(() => funcSelector.Invoke(objCurrent), token);
            }
            return lngReturn;
        }

        public static float Sum<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            float fltReturn = 0;
            foreach (T objCurrent in objEnumerable)
            {
                token.ThrowIfCancellationRequested();
                if (funcPredicate(objCurrent))
                    fltReturn += funcSelector.Invoke(objCurrent);
            }
            return fltReturn;
        }

        public static float Sum<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            float fltReturn = 0;
            foreach (T objCurrent in objEnumerable)
            {
                token.ThrowIfCancellationRequested();
                if (funcPredicate(objCurrent))
                    fltReturn += Utils.SafelyRunSynchronously(() => funcSelector.Invoke(objCurrent), token);
            }
            return fltReturn;
        }

        public static double Sum<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            double dblReturn = 0;
            foreach (T objCurrent in objEnumerable)
            {
                token.ThrowIfCancellationRequested();
                if (funcPredicate(objCurrent))
                    dblReturn += funcSelector.Invoke(objCurrent);
            }
            return dblReturn;
        }

        public static double Sum<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            double dblReturn = 0;
            foreach (T objCurrent in objEnumerable)
            {
                token.ThrowIfCancellationRequested();
                if (funcPredicate(objCurrent))
                    dblReturn += Utils.SafelyRunSynchronously(() => funcSelector.Invoke(objCurrent), token);
            }
            return dblReturn;
        }

        public static decimal Sum<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            decimal decReturn = 0;
            foreach (T objCurrent in objEnumerable)
            {
                token.ThrowIfCancellationRequested();
                if (funcPredicate(objCurrent))
                    decReturn += funcSelector.Invoke(objCurrent);
            }
            return decReturn;
        }

        public static decimal Sum<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            decimal decReturn = 0;
            foreach (T objCurrent in objEnumerable)
            {
                token.ThrowIfCancellationRequested();
                if (funcPredicate(objCurrent))
                    decReturn += Utils.SafelyRunSynchronously(() => funcSelector.Invoke(objCurrent), token);
            }
            return decReturn;
        }

        public static int SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        return funcSelector.Invoke(objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0));

                    default:
                        lstTasks = new List<Task<int>>(Math.Max(Utils.MaxParallelBatchSize, objTemp.Count));
                        break;
                }
            }
            else
                lstTasks = new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (int intLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            intReturn += intLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (int intLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                intReturn += intLoop;
            return intReturn;
        }

        public static int SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        return Utils.SafelyRunSynchronously(() => funcSelector.Invoke(objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0)), token);

                    default:
                        lstTasks = new List<Task<int>>(Math.Max(Utils.MaxParallelBatchSize, objTemp.Count));
                        break;
                }
            }
            else
                lstTasks = new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (int intLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            intReturn += intLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (int intLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                intReturn += intLoop;
            return intReturn;
        }

        public static long SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        return funcSelector.Invoke(objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0));

                    default:
                        lstTasks = new List<Task<long>>(Math.Max(Utils.MaxParallelBatchSize, objTemp.Count));
                        break;
                }
            }
            else
                lstTasks = new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (long longLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            lngReturn += longLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (long longLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                lngReturn += longLoop;
            return lngReturn;
        }

        public static long SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        return Utils.SafelyRunSynchronously(() => funcSelector.Invoke(objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0)), token);

                    default:
                        lstTasks = new List<Task<long>>(Math.Max(Utils.MaxParallelBatchSize, objTemp.Count));
                        break;
                }
            }
            else
                lstTasks = new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (long longLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            lngReturn += longLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (long longLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                lngReturn += longLoop;
            return lngReturn;
        }

        public static float SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        return funcSelector.Invoke(objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0));

                    default:
                        lstTasks = new List<Task<float>>(Math.Max(Utils.MaxParallelBatchSize, objTemp.Count));
                        break;
                }
            }
            else
                lstTasks = new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (float floatLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            fltReturn += floatLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (float floatLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                fltReturn += floatLoop;
            return fltReturn;
        }

        public static float SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        return Utils.SafelyRunSynchronously(() => funcSelector.Invoke(objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0)), token);

                    default:
                        lstTasks = new List<Task<float>>(Math.Max(Utils.MaxParallelBatchSize, objTemp.Count));
                        break;
                }
            }
            else
                lstTasks = new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (float floatLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            fltReturn += floatLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (float floatLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                fltReturn += floatLoop;
            return fltReturn;
        }

        public static double SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        return funcSelector.Invoke(objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0));

                    default:
                        lstTasks = new List<Task<double>>(Math.Max(Utils.MaxParallelBatchSize, objTemp.Count));
                        break;
                }
            }
            else
                lstTasks = new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (double doubleLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            dblReturn += doubleLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (double doubleLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                dblReturn += doubleLoop;
            return dblReturn;
        }

        public static double SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        return Utils.SafelyRunSynchronously(() => funcSelector.Invoke(objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0)), token);

                    default:
                        lstTasks = new List<Task<double>>(Math.Max(Utils.MaxParallelBatchSize, objTemp.Count));
                        break;
                }
            }
            else
                lstTasks = new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (double doubleLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            dblReturn += doubleLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (double doubleLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                dblReturn += doubleLoop;
            return dblReturn;
        }

        public static decimal SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        return funcSelector.Invoke(objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0));

                    default:
                        lstTasks = new List<Task<decimal>>(Math.Max(Utils.MaxParallelBatchSize, objTemp.Count));
                        break;
                }
            }
            else
                lstTasks = new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (decimal decimalLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            decReturn += decimalLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (decimal decimalLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                decReturn += decimalLoop;
            return decReturn;
        }

        public static decimal SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        return Utils.SafelyRunSynchronously(() => funcSelector.Invoke(objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0)), token);

                    default:
                        lstTasks = new List<Task<decimal>>(Math.Max(Utils.MaxParallelBatchSize, objTemp.Count));
                        break;
                }
            }
            else
                lstTasks = new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (decimal decimalLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            decReturn += decimalLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (decimal decimalLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                decReturn += decimalLoop;
            return decReturn;
        }

        public static int SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        T objFirstElement = objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0);
                        return funcPredicate(objFirstElement) ? funcSelector.Invoke(objFirstElement) : 0;

                    default:
                        lstTasks = new List<Task<int>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (int intLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            intReturn += intLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (int intLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                intReturn += intLoop;
            return intReturn;
        }

        public static int SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        T objFirstElement = objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0);
                        return funcPredicate(objFirstElement) ? Utils.SafelyRunSynchronously(() => funcSelector.Invoke(objFirstElement), token) : 0;

                    default:
                        lstTasks = new List<Task<int>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent).ConfigureAwait(false) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (int intLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            intReturn += intLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (int intLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                intReturn += intLoop;
            return intReturn;
        }

        public static long SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        T objFirstElement = objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0);
                        return funcPredicate(objFirstElement) ? funcSelector.Invoke(objFirstElement) : 0;

                    default:
                        lstTasks = new List<Task<long>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (long longLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            lngReturn += longLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (long longLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                lngReturn += longLoop;
            return lngReturn;
        }

        public static long SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        T objFirstElement = objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0);
                        return funcPredicate(objFirstElement) ? Utils.SafelyRunSynchronously(() => funcSelector.Invoke(objFirstElement), token) : 0;

                    default:
                        lstTasks = new List<Task<long>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent).ConfigureAwait(false) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (long longLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            lngReturn += longLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (long longLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                lngReturn += longLoop;
            return lngReturn;
        }

        public static float SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        T objFirstElement = objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0);
                        return funcPredicate(objFirstElement) ? funcSelector.Invoke(objFirstElement) : 0;

                    default:
                        lstTasks = new List<Task<float>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (float floatLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            fltReturn += floatLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (float floatLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                fltReturn += floatLoop;
            return fltReturn;
        }

        public static float SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        T objFirstElement = objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0);
                        return funcPredicate(objFirstElement) ? Utils.SafelyRunSynchronously(() => funcSelector.Invoke(objFirstElement), token) : 0;

                    default:
                        lstTasks = new List<Task<float>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent).ConfigureAwait(false) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (float floatLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            fltReturn += floatLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (float floatLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                fltReturn += floatLoop;
            return fltReturn;
        }

        public static double SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        T objFirstElement = objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0);
                        return funcPredicate(objFirstElement) ? funcSelector.Invoke(objFirstElement) : 0;

                    default:
                        lstTasks = new List<Task<double>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (double doubleLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            dblReturn += doubleLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (double doubleLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                dblReturn += doubleLoop;
            return dblReturn;
        }

        public static double SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        T objFirstElement = objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0);
                        return funcPredicate(objFirstElement) ? Utils.SafelyRunSynchronously(() => funcSelector.Invoke(objFirstElement), token) : 0;

                    default:
                        lstTasks = new List<Task<double>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent).ConfigureAwait(false) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (double doubleLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            dblReturn += doubleLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (double doubleLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                dblReturn += doubleLoop;
            return dblReturn;
        }

        public static decimal SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        T objFirstElement = objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0);
                        return funcPredicate(objFirstElement) ? funcSelector.Invoke(objFirstElement) : 0;

                    default:
                        lstTasks = new List<Task<decimal>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (decimal decimalLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            decReturn += decimalLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (decimal decimalLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                decReturn += decimalLoop;
            return decReturn;
        }

        public static decimal SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        T objFirstElement = objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0);
                        return funcPredicate(objFirstElement) ? Utils.SafelyRunSynchronously(() => funcSelector.Invoke(objFirstElement), token) : 0;

                    default:
                        lstTasks = new List<Task<decimal>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent).ConfigureAwait(false) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (decimal decimalLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            decReturn += decimalLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (decimal decimalLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                decReturn += decimalLoop;
            return decReturn;
        }

        public static int SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        T objFirstElement = objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0);
                        return Utils.SafelyRunSynchronously(() => funcPredicate(objFirstElement), token) ? funcSelector.Invoke(objFirstElement) : 0;

                    default:
                        lstTasks = new List<Task<int>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent).ConfigureAwait(false) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (int intLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            intReturn += intLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (int intLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                intReturn += intLoop;
            return intReturn;
        }

        public static int SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        T objFirstElement = objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0);
                        return Utils.SafelyRunSynchronously(async () => await funcPredicate(objFirstElement).ConfigureAwait(false) ? await funcSelector.Invoke(objFirstElement).ConfigureAwait(false) : 0, token);

                    default:
                        lstTasks = new List<Task<int>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent).ConfigureAwait(false) ? await funcSelector.Invoke(objCurrent).ConfigureAwait(false) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (int intLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            intReturn += intLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (int intLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                intReturn += intLoop;
            return intReturn;
        }

        public static long SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        T objFirstElement = objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0);
                        return Utils.SafelyRunSynchronously(() => funcPredicate(objFirstElement), token) ? funcSelector.Invoke(objFirstElement) : 0;

                    default:
                        lstTasks = new List<Task<long>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent).ConfigureAwait(false) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (long longLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            lngReturn += longLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (long longLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                lngReturn += longLoop;
            return lngReturn;
        }

        public static long SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        T objFirstElement = objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0);
                        return Utils.SafelyRunSynchronously(async () => await funcPredicate(objFirstElement).ConfigureAwait(false) ? await funcSelector.Invoke(objFirstElement).ConfigureAwait(false) : 0, token);

                    default:
                        lstTasks = new List<Task<long>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent).ConfigureAwait(false) ? await funcSelector.Invoke(objCurrent).ConfigureAwait(false) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (long longLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            lngReturn += longLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (long longLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                lngReturn += longLoop;
            return lngReturn;
        }

        public static float SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        T objFirstElement = objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0);
                        return Utils.SafelyRunSynchronously(() => funcPredicate(objFirstElement), token) ? funcSelector.Invoke(objFirstElement) : 0;

                    default:
                        lstTasks = new List<Task<float>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent).ConfigureAwait(false) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (float floatLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            fltReturn += floatLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (float floatLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                fltReturn += floatLoop;
            return fltReturn;
        }

        public static float SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        T objFirstElement = objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0);
                        return Utils.SafelyRunSynchronously(async () => await funcPredicate(objFirstElement).ConfigureAwait(false) ? await funcSelector.Invoke(objFirstElement).ConfigureAwait(false) : 0, token);

                    default:
                        lstTasks = new List<Task<float>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent).ConfigureAwait(false) ? await funcSelector.Invoke(objCurrent).ConfigureAwait(false) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (float floatLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            fltReturn += floatLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (float floatLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                fltReturn += floatLoop;
            return fltReturn;
        }

        public static double SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        T objFirstElement = objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0);
                        return Utils.SafelyRunSynchronously(() => funcPredicate(objFirstElement), token) ? funcSelector.Invoke(objFirstElement) : 0;

                    default:
                        lstTasks = new List<Task<double>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent).ConfigureAwait(false) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (double doubleLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            dblReturn += doubleLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (double doubleLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                dblReturn += doubleLoop;
            return dblReturn;
        }

        public static double SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        T objFirstElement = objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0);
                        return Utils.SafelyRunSynchronously(async () => await funcPredicate(objFirstElement).ConfigureAwait(false) ? await funcSelector.Invoke(objFirstElement).ConfigureAwait(false) : 0, token);

                    default:
                        lstTasks = new List<Task<double>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent).ConfigureAwait(false) ? await funcSelector.Invoke(objCurrent).ConfigureAwait(false) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (double doubleLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            dblReturn += doubleLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (double doubleLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                dblReturn += doubleLoop;
            return dblReturn;
        }

        public static decimal SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        T objFirstElement = objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0);
                        return Utils.SafelyRunSynchronously(() => funcPredicate(objFirstElement), token) ? funcSelector.Invoke(objFirstElement) : 0;

                    default:
                        lstTasks = new List<Task<decimal>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent).ConfigureAwait(false) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (decimal decimalLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            decReturn += decimalLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (decimal decimalLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                decReturn += decimalLoop;
            return decReturn;
        }

        public static decimal SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;

                    case 1:
                        T objFirstElement = objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0);
                        return Utils.SafelyRunSynchronously(async () => await funcPredicate(objFirstElement).ConfigureAwait(false) ? await funcSelector.Invoke(objFirstElement).ConfigureAwait(false) : 0, token);

                    default:
                        lstTasks = new List<Task<decimal>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent).ConfigureAwait(false) ? await funcSelector.Invoke(objCurrent).ConfigureAwait(false) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        foreach (decimal decimalLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                            decReturn += decimalLoop;
                        lstTasks.Clear();
                    }
                }
            }
            foreach (decimal decimalLoop in Utils.SafelyRunSynchronously(() => Task.WhenAll(lstTasks), token))
                decReturn += decimalLoop;
            return decReturn;
        }
    }
}
