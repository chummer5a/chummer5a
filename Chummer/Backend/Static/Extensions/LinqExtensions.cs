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
            return first.GetOrderInvariantEnsembleHashCode() == second.GetOrderInvariantEnsembleHashCode() && first
                .Concat(second).Distinct()
                .All(objItem => first.Count(x => x.Equals(objItem)) == second.Count(x => x.Equals(objItem)));
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
            return first.Count == second.Count
                   && first.GetOrderInvariantEnsembleHashCode() == second.GetOrderInvariantEnsembleHashCode()
                   && first.Concat(second).Distinct().All(objItem =>
                                                              first.Count(x => comparer.Equals(x, objItem))
                                                              == second.Count(x => comparer.Equals(x, objItem)));
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

        public static int SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, int> funcSelector)
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
                        lstTasks = new List<Task<int>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<int>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent)));
                }
            }
            int[] aintReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            int intReturn = 0;
            foreach (int intLoop in aintReturn)
                intReturn += intLoop;
            return intReturn;
        }

        public static int SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<int>> funcSelector)
        {
            List<Task<int>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;
                    case 1:
                        return funcSelector.Invoke(objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0)).GetAwaiter().GetResult();
                    default:
                        lstTasks = new List<Task<int>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<int>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent)));
                }
            }
            int[] aintReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            int intReturn = 0;
            foreach (int intLoop in aintReturn)
                intReturn += intLoop;
            return intReturn;
        }

        public static long SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, long> funcSelector)
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
                        lstTasks = new List<Task<long>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<long>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent)));
                }
            }
            long[] alngReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            long lngReturn = 0;
            foreach (long lngLoop in alngReturn)
                lngReturn += lngLoop;
            return lngReturn;
        }

        public static long SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<long>> funcSelector)
        {
            List<Task<long>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;
                    case 1:
                        return funcSelector.Invoke(objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0)).GetAwaiter().GetResult();
                    default:
                        lstTasks = new List<Task<long>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<long>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent)));
                }
            }
            long[] alngReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            long lngReturn = 0;
            foreach (long lngLoop in alngReturn)
                lngReturn += lngLoop;
            return lngReturn;
        }

        public static float SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, float> funcSelector)
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
                        lstTasks = new List<Task<float>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<float>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent)));
                }
            }
            float[] afltReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            float fltReturn = 0;
            foreach (float fltLoop in afltReturn)
                fltReturn += fltLoop;
            return fltReturn;
        }

        public static float SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<float>> funcSelector)
        {
            List<Task<float>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;
                    case 1:
                        return funcSelector.Invoke(objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0)).GetAwaiter().GetResult();
                    default:
                        lstTasks = new List<Task<float>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<float>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent)));
                }
            }
            float[] afltReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            float fltReturn = 0;
            foreach (float fltLoop in afltReturn)
                fltReturn += fltLoop;
            return fltReturn;
        }

        public static double SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, double> funcSelector)
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
                        lstTasks = new List<Task<double>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<double>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent)));
                }
            }
            double[] adblReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            double dblReturn = 0;
            foreach (double dblLoop in adblReturn)
                dblReturn += dblLoop;
            return dblReturn;
        }

        public static double SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<double>> funcSelector)
        {
            List<Task<double>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;
                    case 1:
                        return funcSelector.Invoke(objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0)).GetAwaiter().GetResult();
                    default:
                        lstTasks = new List<Task<double>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<double>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent)));
                }
            }
            double[] adblReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            double dblReturn = 0;
            foreach (double dblLoop in adblReturn)
                dblReturn += dblLoop;
            return dblReturn;
        }

        public static decimal SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, decimal> funcSelector)
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
                        lstTasks = new List<Task<decimal>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<decimal>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent)));
                }
            }
            decimal[] adecReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            decimal decReturn = 0;
            foreach (decimal decLoop in adecReturn)
                decReturn += decLoop;
            return decReturn;
        }

        public static decimal SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector)
        {
            List<Task<decimal>> lstTasks;
            if (objEnumerable is IReadOnlyCollection<T> objTemp)
            {
                switch (objTemp.Count)
                {
                    case 0:
                        return 0;
                    case 1:
                        return funcSelector.Invoke(objTemp is IReadOnlyList<T> objTemp2 ? objTemp2[0] : objTemp.ElementAt(0)).GetAwaiter().GetResult();
                    default:
                        lstTasks = new List<Task<decimal>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<decimal>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent)));
                }
            }
            decimal[] adecReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            decimal decReturn = 0;
            foreach (decimal decLoop in adecReturn)
                decReturn += decLoop;
            return decReturn;
        }

        public static int Sum<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector)
        {
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate(objEnumerator.Current))
                        intReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return intReturn;
        }

        public static int Sum<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector)
        {
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate(objEnumerator.Current))
                        intReturn += funcSelector.Invoke(objEnumerator.Current).GetAwaiter().GetResult();
                }
            }
            return intReturn;
        }

        public static long Sum<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector)
        {
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate(objEnumerator.Current))
                        lngReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return lngReturn;
        }

        public static long Sum<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector)
        {
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate(objEnumerator.Current))
                        lngReturn += funcSelector.Invoke(objEnumerator.Current).GetAwaiter().GetResult();
                }
            }
            return lngReturn;
        }
        
        public static float Sum<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector)
        {
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate(objEnumerator.Current))
                        fltReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return fltReturn;
        }

        public static float Sum<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector)
        {
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate(objEnumerator.Current))
                        fltReturn += funcSelector.Invoke(objEnumerator.Current).GetAwaiter().GetResult();
                }
            }
            return fltReturn;
        }

        public static double Sum<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector)
        {
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate(objEnumerator.Current))
                        dblReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return dblReturn;
        }

        public static double Sum<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector)
        {
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate(objEnumerator.Current))
                        dblReturn += funcSelector.Invoke(objEnumerator.Current).GetAwaiter().GetResult();
                }
            }
            return dblReturn;
        }

        public static decimal Sum<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector)
        {
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate(objEnumerator.Current))
                        decReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return decReturn;
        }

        public static decimal Sum<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector)
        {
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate(objEnumerator.Current))
                        decReturn += funcSelector.Invoke(objEnumerator.Current).GetAwaiter().GetResult();
                }
            }
            return decReturn;
        }

        public static int SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector)
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
                lstTasks = new List<Task<int>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0));
                }
            }
            int[] aintReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            int intReturn = 0;
            foreach (int intLoop in aintReturn)
                intReturn += intLoop;
            return intReturn;
        }

        public static int SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector)
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
                        return funcPredicate(objFirstElement) ? funcSelector.Invoke(objFirstElement).GetAwaiter().GetResult() : 0;
                    default:
                        lstTasks = new List<Task<int>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<int>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0));
                }
            }
            int[] aintReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            int intReturn = 0;
            foreach (int intLoop in aintReturn)
                intReturn += intLoop;
            return intReturn;
        }

        public static long SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector)
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
                lstTasks = new List<Task<long>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0));
                }
            }
            long[] alngReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            long lngReturn = 0;
            foreach (long lngLoop in alngReturn)
                lngReturn += lngLoop;
            return lngReturn;
        }

        public static long SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector)
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
                        return funcPredicate(objFirstElement) ? funcSelector.Invoke(objFirstElement).GetAwaiter().GetResult() : 0;
                    default:
                        lstTasks = new List<Task<long>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<long>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0));
                }
            }
            long[] alngReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            long lngReturn = 0;
            foreach (long lngLoop in alngReturn)
                lngReturn += lngLoop;
            return lngReturn;
        }

        public static float SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector)
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
                lstTasks = new List<Task<float>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0));
                }
            }
            float[] afltReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            float fltReturn = 0;
            foreach (float fltLoop in afltReturn)
                fltReturn += fltLoop;
            return fltReturn;
        }

        public static float SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector)
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
                        return funcPredicate(objFirstElement) ? funcSelector.Invoke(objFirstElement).GetAwaiter().GetResult() : 0;
                    default:
                        lstTasks = new List<Task<float>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<float>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0));
                }
            }
            float[] afltReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            float fltReturn = 0;
            foreach (float fltLoop in afltReturn)
                fltReturn += fltLoop;
            return fltReturn;
        }

        public static double SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector)
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
                lstTasks = new List<Task<double>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0));
                }
            }
            double[] adblReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            double dblReturn = 0;
            foreach (double dblLoop in adblReturn)
                dblReturn += dblLoop;
            return dblReturn;
        }

        public static double SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector)
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
                        return funcPredicate(objFirstElement) ? funcSelector.Invoke(objFirstElement).GetAwaiter().GetResult() : 0;
                    default:
                        lstTasks = new List<Task<double>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<double>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0));
                }
            }
            double[] adblReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            double dblReturn = 0;
            foreach (double dblLoop in adblReturn)
                dblReturn += dblLoop;
            return dblReturn;
        }
        
        public static decimal SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector)
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
                lstTasks = new List<Task<decimal>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0));
                }
            }
            decimal[] adecReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            decimal decReturn = 0;
            foreach (decimal decLoop in adecReturn)
                decReturn += decLoop;
            return decReturn;
        }

        public static decimal SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector)
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
                        return funcPredicate(objFirstElement) ? funcSelector.Invoke(objFirstElement).GetAwaiter().GetResult() : 0;
                    default:
                        lstTasks = new List<Task<decimal>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<decimal>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0));
                }
            }
            decimal[] adecReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            decimal decReturn = 0;
            foreach (decimal decLoop in adecReturn)
                decReturn += decLoop;
            return decReturn;
        }

        public static int SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector)
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
                        return funcPredicate(objFirstElement).GetAwaiter().GetResult() ? funcSelector.Invoke(objFirstElement) : 0;
                    default:
                        lstTasks = new List<Task<int>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<int>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0));
                }
            }
            int[] aintReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            int intReturn = 0;
            foreach (int intLoop in aintReturn)
                intReturn += intLoop;
            return intReturn;
        }

        public static int SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector)
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
                        return funcPredicate(objFirstElement).GetAwaiter().GetResult() ? funcSelector.Invoke(objFirstElement).GetAwaiter().GetResult() : 0;
                    default:
                        lstTasks = new List<Task<int>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<int>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0));
                }
            }
            int[] aintReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            int intReturn = 0;
            foreach (int intLoop in aintReturn)
                intReturn += intLoop;
            return intReturn;
        }

        public static long SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector)
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
                        return funcPredicate(objFirstElement).GetAwaiter().GetResult() ? funcSelector.Invoke(objFirstElement) : 0;
                    default:
                        lstTasks = new List<Task<long>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<long>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0));
                }
            }
            long[] alngReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            long lngReturn = 0;
            foreach (long lngLoop in alngReturn)
                lngReturn += lngLoop;
            return lngReturn;
        }

        public static long SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector)
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
                        return funcPredicate(objFirstElement).GetAwaiter().GetResult() ? funcSelector.Invoke(objFirstElement).GetAwaiter().GetResult() : 0;
                    default:
                        lstTasks = new List<Task<long>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<long>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0));
                }
            }
            long[] alngReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            long lngReturn = 0;
            foreach (long lngLoop in alngReturn)
                lngReturn += lngLoop;
            return lngReturn;
        }

        public static float SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector)
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
                        return funcPredicate(objFirstElement).GetAwaiter().GetResult() ? funcSelector.Invoke(objFirstElement) : 0;
                    default:
                        lstTasks = new List<Task<float>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<float>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0));
                }
            }
            float[] afltReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            float fltReturn = 0;
            foreach (float fltLoop in afltReturn)
                fltReturn += fltLoop;
            return fltReturn;
        }

        public static float SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector)
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
                        return funcPredicate(objFirstElement).GetAwaiter().GetResult() ? funcSelector.Invoke(objFirstElement).GetAwaiter().GetResult() : 0;
                    default:
                        lstTasks = new List<Task<float>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<float>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0));
                }
            }
            float[] afltReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            float fltReturn = 0;
            foreach (float fltLoop in afltReturn)
                fltReturn += fltLoop;
            return fltReturn;
        }

        public static double SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector)
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
                        return funcPredicate(objFirstElement).GetAwaiter().GetResult() ? funcSelector.Invoke(objFirstElement) : 0;
                    default:
                        lstTasks = new List<Task<double>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<double>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0));
                }
            }
            double[] adblReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            double dblReturn = 0;
            foreach (double dblLoop in adblReturn)
                dblReturn += dblLoop;
            return dblReturn;
        }

        public static double SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector)
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
                        return funcPredicate(objFirstElement).GetAwaiter().GetResult() ? funcSelector.Invoke(objFirstElement).GetAwaiter().GetResult() : 0;
                    default:
                        lstTasks = new List<Task<double>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<double>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0));
                }
            }
            double[] adblReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            double dblReturn = 0;
            foreach (double dblLoop in adblReturn)
                dblReturn += dblLoop;
            return dblReturn;
        }

        public static decimal SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector)
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
                        return funcPredicate(objFirstElement).GetAwaiter().GetResult() ? funcSelector.Invoke(objFirstElement) : 0;
                    default:
                        lstTasks = new List<Task<decimal>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<decimal>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0));
                }
            }
            decimal[] adecReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            decimal decReturn = 0;
            foreach (decimal decLoop in adecReturn)
                decReturn += decLoop;
            return decReturn;
        }

        public static decimal SumParallel<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector)
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
                        return funcPredicate(objFirstElement).GetAwaiter().GetResult() ? funcSelector.Invoke(objFirstElement).GetAwaiter().GetResult() : 0;
                    default:
                        lstTasks = new List<Task<decimal>>(objTemp.Count);
                        break;
                }
            }
            else
                lstTasks = new List<Task<decimal>>();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0));
                }
            }
            decimal[] adecReturn = Utils.RunWithoutThreadLock(() => lstTasks.ToArray());
            decimal decReturn = 0;
            foreach (decimal decLoop in adecReturn)
                decReturn += decLoop;
            return decReturn;
        }
    }
}
