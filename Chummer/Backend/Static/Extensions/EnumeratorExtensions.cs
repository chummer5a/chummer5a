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

using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Chummer
{
    public static class EnumeratorExtensions
    {
        /// <summary>
        /// Syntactic sugar to get a locking version of GetEnumerator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lstEnumerator"></param>
        /// <param name="rwlThis"></param>
        /// <returns></returns>
        public static IEnumerator<T> GetLockingType<T>(this IEnumerator<T> lstEnumerator,
                                                       ReaderWriterLockSlim rwlThis)
        {
            return new LockingEnumerator<T>(lstEnumerator, rwlThis);
        }

        /// <summary>
        /// Syntactic sugar to get a locking version of GetEnumerator
        /// </summary>
        /// <param name="dicCollection"></param>
        /// <param name="rwlThis"></param>
        /// <returns></returns>
        public static IDictionaryEnumerator GetLockingDictionaryEnumerator(this IDictionary dicCollection, ReaderWriterLockSlim rwlThis)
        {
            return new LockingDictionaryEnumerator(dicCollection, rwlThis);
        }
    }
}
