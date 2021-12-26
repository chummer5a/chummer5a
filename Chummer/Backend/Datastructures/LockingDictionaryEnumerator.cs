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
using System.Collections;
using System.Threading;

namespace Chummer
{
    public readonly struct LockingDictionaryEnumerator : IDictionaryEnumerator, IEquatable<LockingDictionaryEnumerator>
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

        /// <inheritdoc />
        public bool Equals(LockingDictionaryEnumerator other)
        {
            return Equals(_rwlThis, other._rwlThis) && Equals(_objInternalEnumerator, other._objInternalEnumerator);
        }

        public override bool Equals(object obj)
        {
            return obj != null && Equals((LockingDictionaryEnumerator)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_rwlThis != null ? _rwlThis.GetHashCode() : 0) * 397) ^ (_objInternalEnumerator != null ? _objInternalEnumerator.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return _objInternalEnumerator.ToString() + ' ' + _rwlThis;
        }

        public static bool operator ==(LockingDictionaryEnumerator left, LockingDictionaryEnumerator right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LockingDictionaryEnumerator left, LockingDictionaryEnumerator right)
        {
            return !(left == right);
        }
    }
}
