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
using System.Threading;

namespace Chummer
{
    public readonly struct LockingDictionaryEnumerator : IDictionaryEnumerator
    {
        private readonly ReaderWriterLockSlim _rwlThis;

        private readonly IDictionaryEnumerator _objInternalEnumerator;

        public LockingDictionaryEnumerator(IDictionary dicEnumerable, ReaderWriterLockSlim rwlThis)
        {
            _objInternalEnumerator = dicEnumerable.GetEnumerator();
            _rwlThis = rwlThis;
        }

        public LockingDictionaryEnumerator(IDictionaryEnumerator lstEnumerator, ReaderWriterLockSlim rwlThis)
        {
            _objInternalEnumerator = lstEnumerator;
            _rwlThis = rwlThis;
        }

        /// <inheritdoc />
        public bool MoveNext()
        {
            using (new EnterReadLock(_rwlThis))
                return _objInternalEnumerator.MoveNext();
        }

        /// <inheritdoc />
        public void Reset()
        {
            using (new EnterReadLock(_rwlThis))
                _objInternalEnumerator.Reset();
        }

        public override bool Equals(object obj)
        {
            return obj != null && _objInternalEnumerator.Equals(((LockingDictionaryEnumerator)obj)._objInternalEnumerator)
                               && _rwlThis.Equals(((LockingDictionaryEnumerator)obj)._rwlThis);
        }

        public override int GetHashCode()
        {
            return (_objInternalEnumerator, _rwlThis).GetHashCode();
        }

        public override string ToString()
        {
            return _objInternalEnumerator.ToString() + ' ' + _rwlThis;
        }

        /// <inheritdoc />
        public object Current
        {
            get
            {
                using (new EnterReadLock(_rwlThis))
                    return _objInternalEnumerator.Current;
            }
        }

        /// <inheritdoc />
        public object Key
        {
            get
            {
                using (new EnterReadLock(_rwlThis))
                    return _objInternalEnumerator.Key;
            }
        }

        /// <inheritdoc />
        public object Value
        {
            get
            {
                using (new EnterReadLock(_rwlThis))
                    return _objInternalEnumerator.Value;
            }
        }

        /// <inheritdoc />
        public DictionaryEntry Entry
        {
            get
            {
                using (new EnterReadLock(_rwlThis))
                    return _objInternalEnumerator.Entry;
            }
        }
    }
}
