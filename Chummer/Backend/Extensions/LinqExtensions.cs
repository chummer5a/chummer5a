using System;
using System.Collections.Generic;
using System.Linq;

namespace Chummer.Backend.Extensions
{
    static class LinqExtensions
    {
        /// <summary>
        /// Similar to Linq's Aggregate(), but deep searches the list, applying the aggregator to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static TSource DeepAggregate<TSource>(this IEnumerable<TSource> objParentList, Func<TSource, IEnumerable<TSource>> funcGetChildrenMethod, Func<TSource, TSource, TSource> funcAggregate)
        {
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
            return resultSelector(objParentList.DeepAggregate(funcGetChildrenMethod, seed, funcAggregate));
        }

        /// <summary>
        /// Similar to Linq's All(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static bool DeepAll<T>(this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
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
        public static bool DeepAny<T>(this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
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
        public static T DeepFirst<T>(this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            T objReturn = default(T);
            foreach (T objLoopChild in objParentList)
            {
                if (predicate(objLoopChild))
                    return objLoopChild;
                objReturn = funcGetChildrenMethod(objLoopChild).DeepFirstOrDefault(funcGetChildrenMethod, predicate);
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
            T objReturn = default(T);
            foreach (T objLoopChild in objParentList)
            {
                if (predicate(objLoopChild))
                    return objLoopChild;
                objReturn = funcGetChildrenMethod(objLoopChild).DeepFirstOrDefault(funcGetChildrenMethod, predicate);
                if (objReturn?.Equals(default(T)) == false)
                    return objReturn;
            }
            return default(T);
        }

        /// <summary>
        /// Similar to Linq's Last(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static T DeepLast<T>(this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
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
            T objReturn = default(T);
            T objTemp = default(T);
            foreach (T objLoopChild in objParentList)
            {
                if (predicate(objLoopChild))
                    objReturn = objLoopChild;
                objTemp = funcGetChildrenMethod(objLoopChild).DeepLastOrDefault(funcGetChildrenMethod, predicate);
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
            T objReturn = objParentList.LastOrDefault();
            T objTemp = funcGetChildrenMethod(objReturn).DeepLastOrDefault(funcGetChildrenMethod);
            if (objTemp?.Equals(default(T)) == false)
                return objTemp;
            return objReturn;
        }

        /// <summary>
        /// Similar to Linq's Where(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static IEnumerable<T> DeepWhere<T>(this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            List<T> objReturn = new List<T>();
            foreach (T objLoopChild in objParentList)
            {
                if (predicate(objLoopChild))
                    objReturn.Add(objLoopChild);
                objReturn.AddRange(funcGetChildrenMethod(objLoopChild).DeepWhere(funcGetChildrenMethod, predicate));
            }
            return objReturn;
        }

        /// <summary>
        /// Gets all relatives in the list, including the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static IEnumerable<T> GetAllDescendants<T>(this IEnumerable<T> objParentList, Func<T, IEnumerable<T>> funcGetChildrenMethod)
        {
            List<T> objReturn = new List<T>();
            foreach (T objLoopChild in objParentList)
            {
                objReturn.Add(objLoopChild);
                objReturn.AddRange(funcGetChildrenMethod(objLoopChild).GetAllDescendants(funcGetChildrenMethod));
            }
            return objReturn;
        }
    }
}
