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
using System.Reflection;

namespace Chummer
{
    internal static class DictionaryExtensions
    {
        public static Dictionary<string, object> ToDictionaryOfPublicMembers(this object objSource)
        {
            if (objSource == null)
                throw new ArgumentNullException(nameof(objSource));
            Dictionary<string, object> dicReturn = new Dictionary<string, object>();
            Type objSourceType = objSource.GetType();
            foreach (FieldInfo objLoopInfo in objSourceType.GetFields(BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance))
            {
                dicReturn.Add(objLoopInfo.Name, objLoopInfo.GetValue(objSource) ?? string.Empty);
            }
            foreach (PropertyInfo objLoopInfo in objSourceType.GetProperties(BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance))
            {
                dicReturn.Add(objLoopInfo.Name, objLoopInfo.GetValue(objSource, null) ?? string.Empty);
            }
            return dicReturn;
        }

        public static bool EqualsByValue(this IDictionary<object, IComparable> dicLeft, IDictionary<object, IComparable> dicRight)
        {
            if (dicLeft.Count != dicRight.Count)
                return false;
            if (dicLeft.Keys.Any(x => !dicRight.ContainsKey(x)))
                return false;
            return dicRight.Keys.All(x => dicLeft.ContainsKey(x) && dicRight[x].Equals(dicLeft[x]));
        }

        public static bool IsAnonymousType(this object objInstance)
        {
            if (objInstance == null)
                return false;
            return objInstance.GetType().Namespace == null;
        }

        public static Dictionary<string, object> ToFlattenedDictionaryOfPublicMembers(this object objSource, string strParentPropertyKey = "", IDictionary<string, object> dicParentPropertyValue = null)
        {
            Dictionary<string, object> dicReturn = dicParentPropertyValue?.ToDictionary(x => x.Key, y => y.Value) ?? new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> objItem in objSource.ToDictionaryOfPublicMembers())
            {
                string strKey = string.IsNullOrEmpty(strParentPropertyKey) ? objItem.Key : $"{strParentPropertyKey}.{objItem.Key}";
                if (objItem.Value.IsAnonymousType())
                {
                    foreach (KeyValuePair<string, object> objInnerItem in objItem.Value
                        .ToFlattenedDictionaryOfPublicMembers(strKey, dicReturn))
                        dicReturn.Add(objInnerItem.Key, objInnerItem.Value);
                }
                else
                    dicReturn.Add(strKey, objItem.Value);
            }
            return dicReturn;
        }
    }
}
