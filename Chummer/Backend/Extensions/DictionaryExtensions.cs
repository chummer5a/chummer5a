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
        public static T ToFlattenedDictionaryOfPublicMembers<T>(this object objSource, string strParentPropertyKey = "", T dicParentPropertyValue = default) where T : IDictionary<string, object>, new()
        {
            T dicReturn = new T();
            if (!dicParentPropertyValue.Equals(default))
            {
                foreach (KeyValuePair<string, object> kvpItem in dicParentPropertyValue)
                    dicReturn.Add(kvpItem.Key, kvpItem.Value);
            }
            foreach (KeyValuePair<string, object> objItem in objSource.ToDictionaryOfPublicMembers<Dictionary<string, object>>())
            {
                string strKey = string.IsNullOrEmpty(strParentPropertyKey) ? objItem.Key : strParentPropertyKey + '.' + objItem.Key;
                if (IsAnonymousType(objItem.Value))
                {
                    foreach (KeyValuePair<string, object> objInnerItem in objItem.Value
                        .ToFlattenedDictionaryOfPublicMembers(strKey, dicReturn))
                        dicReturn.Add(objInnerItem.Key, objInnerItem.Value);
                }
                else
                    dicReturn.Add(strKey, objItem.Value);
            }
            return dicReturn;

            bool IsAnonymousType(object objInstance)
            {
                if (objInstance == null)
                    return false;
                return objInstance.GetType().Namespace == null;
            }
        }

        public static T ToDictionaryOfPublicMembers<T>(this object objSource) where T : IDictionary<string, object>, new()
        {
            if (objSource == null)
                throw new ArgumentNullException(nameof(objSource));
            Type objSourceType = objSource.GetType();
            T dicReturn = new T();
            foreach (FieldInfo objLoopInfo in objSourceType.GetFields(BindingFlags.GetField | BindingFlags.Public |
                                                                      BindingFlags.Instance))
                dicReturn.Add(objLoopInfo.Name, objLoopInfo.GetValue(objSource) ?? string.Empty);
            foreach (PropertyInfo objLoopInfo in objSourceType.GetProperties(BindingFlags.GetField |
                                                                             BindingFlags.GetProperty |
                                                                             BindingFlags.Public |
                                                                             BindingFlags.Instance))
                dicReturn.Add(objLoopInfo.Name, objLoopInfo.GetValue(objSource, null) ?? string.Empty);
            return dicReturn;
        }
    }
}
