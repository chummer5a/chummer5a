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
using System.Threading.Tasks;

namespace Chummer
{
    public sealed class LockingDictionaryEnumerator : IDictionaryEnumerator, IDisposable
    {
        private readonly IHasLockObject _objMyParent;

        private IDictionaryEnumerator _objInternalEnumerator;

        public static LockingDictionaryEnumerator Get(IHasLockObject objMyParent)
        {
            objMyParent.LockObject.EnterReadLock();
            return new LockingDictionaryEnumerator(objMyParent);
        }

        public static async ValueTask<LockingDictionaryEnumerator> GetAsync(IHasLockObject objMyParent)
        {
            await objMyParent.LockObject.EnterReadLockAsync();
            return new LockingDictionaryEnumerator(objMyParent);
        }

        private LockingDictionaryEnumerator(IHasLockObject objMyParent)
        {
            _objMyParent = objMyParent;
        }

        public void SetEnumerator(IDictionaryEnumerator objInternalEnumerator)
        {
            if (_objInternalEnumerator != null)
                throw new ArgumentException(null, nameof(objInternalEnumerator));
            _objInternalEnumerator = objInternalEnumerator;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _objMyParent.LockObject.ExitReadLock();
        }

        /// <inheritdoc />
        public bool MoveNext()
        {
            return _objInternalEnumerator.MoveNext();
        }

        /// <inheritdoc />
        public void Reset()
        {
            _objInternalEnumerator.Reset();
        }

        /// <inheritdoc />
        public object Current => _objInternalEnumerator.Current;

        /// <inheritdoc />
        public object Key => _objInternalEnumerator.Key;

        /// <inheritdoc />
        public object Value => _objInternalEnumerator.Value;

        /// <inheritdoc />
        public DictionaryEntry Entry => _objInternalEnumerator.Entry;
    }
}
