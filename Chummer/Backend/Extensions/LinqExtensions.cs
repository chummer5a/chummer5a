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
        /// Similar to Linq's Aggregate(), but deep searches the list, applying the aggregator to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static TSource DeepAggregate<TSource>(this IEnumerable<TSource> objParentList, Func<TSource, IEnumerable<TSource>> funcGetChildrenMethod, Func<TSource, TSource, TSource> funcAggregate)
        {
            if (objParentList == null)
                return default(TSource);
            TSource objReturn = default(TSource);
            foreach (TSource objLoopChild in objParentList)
            {
                objReturn = funcAggregate(funcAggregate(objReturn, objLoopChild), funcGetChildrenMethod(objLoopChild).DeepAggregate(funcGetChildrenMethod, funcAggregate));
            }
            return objReturn;
        }

        /// <summary>
        /// Similar to Linq's Aggregate(), but deep searches the list, applying the aggregator to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static TAccumulate DeepAggregate<TSource, TAccumulate>(this IEnumerable<TSource> objParentList, Func<TSource, IEnumerable<TSource>> funcGetChildrenMethod, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> funcAggregate)
        {
            if (objParentList == null)
                return seed;
            TAccumulate objReturn = seed;
            foreach (TSource objLoopChild in objParentList)
            {
                objReturn = funcGetChildrenMethod(objLoopChild).DeepAggregate(funcGetChildrenMethod, funcAggregate(objReturn, objLoopChild), funcAggregate);
            }
            return objReturn;
        }

        /// <summary>
        /// Similar to Linq's Aggregate(), but deep searches the list, applying the aggregator to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static TResult DeepAggregate<TSource, TAccumulate, TResult>(this IEnumerable<TSource> objParentList, Func<TSource, IEnumerable<TSource>> funcGetChildrenMethod, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> funcAggregate, Func<TAccumulate, TResult> resultSelector)
        {
            if (resultSelector == null)
                return default(TResult);
            return resultSelector(objParentList.DeepAggregate(funcGetChildrenMethod, seed, funcAggregate));
        }

        /// <summary>
        /// Deep searches two collections to make sure matching elements between the two fulfill some predicate. Each item in the target list can only be matched once.
        /// </summary>
        /// <param name="objParentList">Base list to check.</param>
        /// <param name="objTargetList">Target list against which we're checking</param>
        /// <param name="funcGetChildrenMethod">Method used to get children of both the base list and the target list against which we're checking.</param>
        /// <param name="predicate">Two-argument function that takes its first argument from the base list and the second from the target list. If it does not return true on any available pair, the method returns false.</param>
        public static bool DeepMatch<T>(this ICollection<T> objParentList, ICollection<T> objTargetList, Func<T, ICollection<T>> funcGetChildrenMethod, Func<T, T, bool> predicate)
        {
            if (objParentList == null && objTargetList == null)
                return true;
            if (objParentList?.Count != objTargetList?.Count)
                return false;
            HashSet<T> lstExclude = new HashSet<T>();
            foreach (T objLoopChild in objParentList)
            {
                foreach (T objTargetChild in objTargetList)
                {
                    if (!lstExclude.Contains(objTargetChild) && predicate(objLoopChild, objTargetChild) && funcGetChildrenMethod(objLoopChild).DeepMatch(funcGetChildrenMethod(objTargetChild), funcGetChildrenMethod, predicate))
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
        /// Similar to Linq's All(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static bool DeepAll<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            if (objParentList == null)
                throw new ArgumentNullException(nameof(objParentList));
            foreach (T objLoopChild in objParentList)
            {
                if (!predicate(objLoopChild) || !funcGetChildrenMethod(objLoopChild).DeepAll(funcGetChildrenMethod, predicate))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Similar to Linq's Any(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static bool DeepAny<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            if (objParentList == null)
                throw new ArgumentNullException(nameof(objParentList));
            foreach (T objLoopChild in objParentList)
            {
                if (predicate(objLoopChild) || funcGetChildrenMethod(objLoopChild).DeepAny(funcGetChildrenMethod, predicate))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Similar to Linq's Count(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
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
        /// Similar to Linq's Count() without predicate, but deep searches the list, counting up the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static int DeepCount<T>(this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod)
        {
            if (objParentList == null)
                return 0;
            int intReturn = 0;
            foreach (T objLoopChild in objParentList)
            {
                intReturn += 1 + funcGetChildrenMethod(objLoopChild).DeepCount(funcGetChildrenMethod);
            }
            return intReturn;
        }

        /// <summary>
        /// Similar to Linq's First(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static T DeepFirst<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            if (objParentList == null)
                throw new ArgumentNullException(nameof(objParentList));
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
        /// Similar to Linq's FirstOrDefault(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static T DeepFirstOrDefault<T>(this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            if (objParentList == null)
                return default(T);
            foreach (T objLoopChild in objParentList)
            {
                if (predicate(objLoopChild))
                    return objLoopChild;
                T objReturn = funcGetChildrenMethod(objLoopChild).DeepFirstOrDefault(funcGetChildrenMethod, predicate);
                if (objReturn?.Equals(default(T)) == false)
                    return objReturn;
            }
            return default(T);
        }

        /// <summary>
        /// Similar to Linq's Last(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static T DeepLast<T>([NotNull] this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            if (objParentList == null)
                throw new ArgumentNullException(nameof(objParentList));
            T objReturn = objParentList.DeepLastOrDefault(funcGetChildrenMethod, predicate);
            if (objReturn?.Equals(default(T)) == false)
                return objReturn;
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to Linq's LastOrDefault(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static T DeepLastOrDefault<T>(this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            if (objParentList == null)
                return default(T);
            T objReturn = default(T);
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
        /// Similar to Linq's Last() without a predicate, but deep searches the list, returning the last element out of the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static T DeepLast<T>(this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod)
        {
            if (objParentList == null)
                throw new ArgumentNullException(nameof(objParentList));
            T objReturn = objParentList.DeepLastOrDefault(funcGetChildrenMethod);
            if (objReturn?.Equals(default(T)) == false)
                return objReturn;
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to Linq's LastOrDefault() without a predicate, but deep searches the list, returning the last element out of the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static T DeepLastOrDefault<T>(this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod)
        {
            if (objParentList == null)
                return default(T);
            T objReturn = objParentList.LastOrDefault();
            if (funcGetChildrenMethod != null && funcGetChildrenMethod(objReturn).Any())
            {
                T objTemp = funcGetChildrenMethod(objReturn).DeepLastOrDefault(funcGetChildrenMethod);
                if (objTemp?.Equals(default(T)) == false)
                    return objTemp;
            }
            return objReturn;
        }

        /// <summary>
        /// Similar to Linq's Where(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static IEnumerable<T> DeepWhere<T>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            if (objParentList == null)
                throw new ArgumentNullException(nameof(objParentList));
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
            if (objParentList == null)
                throw new ArgumentNullException(nameof(objParentList));
            foreach (T objLoopChild in objParentList)
            {
                yield return objLoopChild;

                foreach (T objLoopSubchild in funcGetChildrenMethod(objLoopChild).GetAllDescendants(funcGetChildrenMethod))
                    yield return objLoopSubchild;
            }
        }
    }
}
