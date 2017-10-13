using System;
using System.Collections.Generic;
using System.Linq;

namespace Chummer.Backend.Extensions
{
    static class LinqExtensions
    {
        /// <summary>
        /// Similar to Linq's Count(), but deep searches the parent, applying the predicate to the parent's children, its children's children, etc.
        /// </summary>
        public static int DeepCountChildren<T>(this IHasChildren<T> objParent, Func<T, bool> predicate) where T : IHasChildren<T>
        {
            int intReturn = objParent.Children.Count(predicate);
            foreach (IHasChildren<T> objLoopChild in objParent.Children)
            {
                intReturn += objLoopChild.DeepCountChildren(predicate);
            }
            return intReturn;
        }

        /*
         * WIP version of the generic form of the above, where I can could use a function pointer instead of having to rely on IHasChildren<T> (and why this file is called LinqExtensions
        /// <summary>
        /// Similar to Linq's Count(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static int DeepCount<T>(this IEnumerable<T> objParentList, Func<IEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            int intReturn = objParentList.Count(predicate);
            foreach (IHasChildren<T> objLoopChild in objParentList)
            {
                intReturn += funcGetChildrenMethod().DeepCount(funcGetChildrenMethod, predicate);
            }
            return intReturn;
        }
         */

        /// <summary>
        /// Similar to Linq's FirstOrDefault(), but deep searches the parent, applying the predicate to the parent's children, its children's children, etc.
        /// </summary>
        public static T DeepFirstOrDefault<T>(this IHasChildren<T> objParent, Func<T, bool> predicate) where T : IHasChildren<T>
        {
            T objReturn = objParent.Children.FirstOrDefault(predicate);
            if (objReturn?.Equals(default(T)) == false)
                return objReturn;
            foreach (IHasChildren<T> objLoopChild in objParent.Children)
            {
                objReturn = objLoopChild.DeepFirstOrDefault(predicate);
                if (objReturn?.Equals(default(T)) == false)
                    return objReturn;
            }
            return default(T);
        }

        /// <summary>
        /// Similar to Linq's Where(), but deep searches the parent, applying the predicate to the parent's children, its children's children, etc.
        /// </summary>
        public static IEnumerable<T> DeepWhere<T>(this IHasChildren<T> objParent, Func<T, bool> predicate) where T : IHasChildren<T>
        {
            List<T> objReturn = new List<T>();
            objReturn.AddRange(objParent.Children.Where(predicate));
            foreach (IHasChildren<T> objLoopChild in objParent.Children)
            {
                objReturn.AddRange(objLoopChild.DeepWhere(predicate));
            }
            return objReturn;
        }

        /// <summary>
        /// Similar to Linq's Where(), but deep searches the parent, applying the predicate to the parent's children, its children's children, etc.
        /// </summary>
        public static IEnumerable<T> GetAllDescendants<T>(this IHasChildren<T> objParent) where T : IHasChildren<T>
        {
            List<T> objReturn = new List<T>();
            objReturn.AddRange(objParent.Children);
            foreach (IHasChildren<T> objLoopChild in objParent.Children)
            {
                objReturn.AddRange(objLoopChild.GetAllDescendants());
            }
            return objReturn;
        }
    }
}
