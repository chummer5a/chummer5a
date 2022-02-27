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
using System.Runtime.CompilerServices;
using System.Threading;

namespace Chummer
{
    /// <summary>
    /// Syntactic Sugar for wrapping a ReaderWriterLockSlim's EnterWriteLock() and ExitWriteLock() methods into something that hooks into `using`
    /// </summary>
    public sealed class EnterWriteLock : IDisposable
    {
        private readonly ReaderWriterLockSlim _rwlMyLock;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EnterWriteLock(ReaderWriterLockSlim rwlMyLock)
        {
            _rwlMyLock = rwlMyLock;
#if DEBUG
            try
            {
                _rwlMyLock.EnterWriteLock();
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
            _rwlMyLock.EnterWriteLock();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EnterWriteLock(IHasLockObject rwlMyLock)
        {
            _rwlMyLock = rwlMyLock.LockObject;
#if DEBUG
            try
            {
                _rwlMyLock.EnterWriteLock();
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
            _rwlMyLock.EnterWriteLock();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
#if DEBUG
            try
            {
                _rwlMyLock.ExitWriteLock();
            }
            catch (SynchronizationLockException ex)
            {
                Utils.BreakIfDebug();
                throw;
            }
#else
            _rwlMyLock.ExitWriteLock();
#endif
        }
    }
}
