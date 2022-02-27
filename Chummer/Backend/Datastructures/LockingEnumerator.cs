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
using System.Collections.Generic;
using System.Threading;

namespace Chummer
{
    /// <summary>
    /// Special class that should be used instead of GetEnumerator for collections with an internal locking object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class LockingEnumerator<T> : IEnumerator<T>
    {
        private readonly IHasLockObject _objMyParent;

        private IEnumerator<T> _objInternalEnumerator;

        public LockingEnumerator(IHasLockObject objMyParent)
        {
            _objMyParent = objMyParent;
#if DEBUG
            try
            {
                _objMyParent.LockObject.EnterReadLock();
            }
            catch (LockRecursionException ex)
            {
                Utils.BreakIfDebug();
                throw;
            }
            catch (ObjectDisposedException ex)
            {
                Utils.BreakIfDebug();
                throw;
            }
#else
            _objMyParent.LockObject.EnterReadLock();
#endif
        }

        public void SetEnumerator(IEnumerator<T> objInternalEnumerator)
        {
            if (_objInternalEnumerator != null)
                throw new ArgumentException(null, nameof(objInternalEnumerator));
            _objInternalEnumerator = objInternalEnumerator;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _objInternalEnumerator.Dispose();
#if DEBUG
            try
            {
                _objMyParent.LockObject.ExitReadLock();
            }
            catch (SynchronizationLockException ex)
            {
                Utils.BreakIfDebug();
                throw;
            }
#else
            _objMyParent.LockObject.ExitReadLock();
#endif
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
        public T Current => _objInternalEnumerator.Current;

        /// <inheritdoc />
        object IEnumerator.Current => Current;
    }
}
