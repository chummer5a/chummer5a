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
using System.Threading;

namespace Chummer
{
    public static class ReaderWriterLockSlimExtensions
    {
        /// <summary>
        /// Version of <see cref="ReaderWriterLockSlim.EnterReadLock()"/> that also processes application events if this is called on the UI thread
        /// </summary>
        public static void SafeEnterReadLock(this ReaderWriterLockSlim rwlLockerObject, bool blnForceDoEvents = false)
        {
            if (Utils.IsUnitTest)
            {
                if (Utils.EverDoEvents)
                {
                    int intLoopCount = 0;
                    while (!rwlLockerObject.TryEnterReadLock(Utils.DefaultSleepDuration))
                    {
                        if (intLoopCount++ > Utils.WaitEmergencyReleaseMaxTicks)
                            throw new TimeoutException();
                        Utils.DoEventsSafe(blnForceDoEvents);
                    }
                }
                else if (!rwlLockerObject.TryEnterReadLock(Utils.WaitEmergencyReleaseMaxTicks))
                    throw new TimeoutException();
            }
            else if (Utils.EverDoEvents)
            {
                while (!rwlLockerObject.TryEnterReadLock(Utils.DefaultSleepDuration))
                    Utils.DoEventsSafe(blnForceDoEvents);
            }
            else
                rwlLockerObject.EnterReadLock();
        }

        /// <summary>
        /// Version of <see cref="ReaderWriterLockSlim.EnterUpgradeableReadLock()"/> that also processes application events if this is called on the UI thread
        /// </summary>
        public static void SafeEnterUpgradeableReadLock(this ReaderWriterLockSlim rwlLockerObject, bool blnForceDoEvents = false)
        {
            if (Utils.IsUnitTest)
            {
                if (Utils.EverDoEvents)
                {
                    int intLoopCount = 0;
                    while (!rwlLockerObject.TryEnterUpgradeableReadLock(Utils.DefaultSleepDuration))
                    {
                        if (intLoopCount++ > Utils.WaitEmergencyReleaseMaxTicks)
                            throw new TimeoutException();
                        Utils.DoEventsSafe(blnForceDoEvents);
                    }
                }
                else if (!rwlLockerObject.TryEnterUpgradeableReadLock(Utils.WaitEmergencyReleaseMaxTicks))
                    throw new TimeoutException();
            }
            else if (Utils.EverDoEvents)
            {
                while (!rwlLockerObject.TryEnterUpgradeableReadLock(Utils.DefaultSleepDuration))
                    Utils.DoEventsSafe(blnForceDoEvents);
            }
            else
                rwlLockerObject.EnterUpgradeableReadLock();
        }

        /// <summary>
        /// Version of <see cref="ReaderWriterLockSlim.EnterWriteLock()"/> that also processes application events if this is called on the UI thread
        /// </summary>
        public static void SafeEnterWriteLock(this ReaderWriterLockSlim rwlLockerObject, bool blnForceDoEvents = false)
        {
            if (Utils.IsUnitTest)
            {
                if (Utils.EverDoEvents)
                {
                    int intLoopCount = 0;
                    while (!rwlLockerObject.TryEnterWriteLock(Utils.DefaultSleepDuration))
                    {
                        if (intLoopCount++ > Utils.WaitEmergencyReleaseMaxTicks)
                            throw new TimeoutException();
                        Utils.DoEventsSafe(blnForceDoEvents);
                    }
                }
                else if (!rwlLockerObject.TryEnterWriteLock(Utils.WaitEmergencyReleaseMaxTicks))
                    throw new TimeoutException();
            }
            else if (Utils.EverDoEvents)
            {
                while (!rwlLockerObject.TryEnterWriteLock(Utils.DefaultSleepDuration))
                    Utils.DoEventsSafe(blnForceDoEvents);
            }
            else
                rwlLockerObject.EnterWriteLock();
        }
    }
}
