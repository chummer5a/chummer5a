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
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chummer.Tests
{
    [TestClass]
    public class A01_LockTests // Weird name is to make sure the tests methods herein run before most other tests in a batch run of all tests, since it's important to identify locking issues early
    {
        public TestContext TestContext { get; set; }

        [TestMethod, Timeout(10 * 1000, CooperativeCancellation = true)]
        public void AsyncFriendlyReaderWriterLock_ReadLockExcludesWriteLockButNotUpgradeableRead()
        {
            using (AsyncFriendlyReaderWriterLock objLock = new AsyncFriendlyReaderWriterLock())
            {
                int intStage = 0;
                Parallel.Invoke(
                    () =>
                    {
                        using (objLock.EnterReadLock(TestContext.CancellationToken))
                        {
                            Interlocked.Increment(ref intStage); // Signal the other thread that the read lock has been acquired
                            while (intStage < 2) // Wait for other thread to signal this read lock should be released
                                Utils.SafeSleep(TestContext.CancellationToken);
                        }
                    },
                    () =>
                    {
                        try
                        {
                            while (intStage == 0) // Wait for read lock to be acquired in the other thread
                                Utils.SafeSleep(TestContext.CancellationToken);
                            using (objLock.EnterUpgradeableReadLock(TestContext.CancellationToken))
                            {
                                try
                                {
                                    using (CancellationTokenSource objSource = new CancellationTokenSource(100))
                                    {
                                        try
                                        {
                                            using (objLock.EnterWriteLock(objSource.Token))
                                                Assert.Fail("Can acquire write lock while a read lock is active.");
                                        }
                                        catch (OperationCanceledException)
                                        {
                                            // Swallow this, this is intended behavior
                                        }
                                    }
                                }
                                finally
                                {
                                    Interlocked.Increment(ref intStage); // Signal the other thread to release the read lock
                                }
                                using (objLock.EnterWriteLock(TestContext.CancellationToken))
                                {
                                    GC.KeepAlive(objLock); // No-op to make sure this doesn't get optimized away
                                }
                            }
                        }
                        finally
                        {
                            Interlocked.Increment(ref intStage); // If we get an exception in this thread, allow the other thread to release its lock and exit early
                        }
                    });
            }
        }

        [TestMethod, Timeout(10 * 1000, CooperativeCancellation = true)]
        public void AsyncFriendlyReaderWriterLock_LocksAreReentrantAndUpgradeProperly()
        {
            using (AsyncFriendlyReaderWriterLock objLock = new AsyncFriendlyReaderWriterLock())
            {
                EnterReadLockRecursively(0, TestContext.CancellationToken);
                EnterUpgradeableReadLockRecursively(0, TestContext.CancellationToken);

                // Make sure the async versions also work
                Utils.SafelyRunSynchronously(async t =>
                {
                    await EnterReadLockRecursivelyAsync(0, t).ConfigureAwait(false);
                    await EnterUpgradeableReadLockRecursivelyAsync(0, t).ConfigureAwait(false);
                }, TestContext.CancellationToken);

                void EnterReadLockRecursively(int intLevel, CancellationToken token = default)
                {
                    token.ThrowIfCancellationRequested();
                    using (objLock.EnterReadLock(token))
                    {
                        if (++intLevel < 2)
                            EnterReadLockRecursively(intLevel, token);
                    }
                }

                void EnterUpgradeableReadLockRecursively(int intLevel, CancellationToken token = default)
                {
                    token.ThrowIfCancellationRequested();
                    using (objLock.EnterUpgradeableReadLock(token))
                    {
                        if (++intLevel < 5)
                        {
                            if ((intLevel & 1) == 0)
                                EnterWriteLockRecursively(intLevel, token);
                            else
                                EnterUpgradeableReadLockRecursively(intLevel, token);
                        }
                        else
                        {
                            EnterReadLockRecursively(intLevel, token);
                        }
                    }
                }

                void EnterWriteLockRecursively(int intLevel, CancellationToken token = default)
                {
                    token.ThrowIfCancellationRequested();
                    using (objLock.EnterWriteLock(token))
                    {
                        if (++intLevel < 5)
                        {
                            if ((intLevel & 1) == 0)
                                EnterWriteLockRecursively(intLevel, token);
                            else
                                EnterUpgradeableReadLockRecursively(intLevel, token);
                        }
                        else
                        {
                            EnterReadLockRecursively(intLevel, token);
                        }
                    }
                }

                async Task EnterReadLockRecursivelyAsync(int intLevel, CancellationToken token = default)
                {
                    token.ThrowIfCancellationRequested();
                    IAsyncDisposable objLocker = await objLock.EnterReadLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (++intLevel < 2)
                            await EnterReadLockRecursivelyAsync(intLevel, token).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }

                async Task EnterUpgradeableReadLockRecursivelyAsync(int intLevel, CancellationToken token = default)
                {
                    token.ThrowIfCancellationRequested();
                    IAsyncDisposable objLocker = await objLock.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (++intLevel < 5)
                        {
                            if ((intLevel & 1) == 0)
                                await EnterWriteLockRecursivelyAsync(intLevel, token).ConfigureAwait(false);
                            else
                                await EnterUpgradeableReadLockRecursivelyAsync(intLevel, token).ConfigureAwait(false);
                        }
                        else
                        {
                            await EnterReadLockRecursivelyAsync(intLevel, token).ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }

                async Task EnterWriteLockRecursivelyAsync(int intLevel, CancellationToken token = default)
                {
                    token.ThrowIfCancellationRequested();
                    IAsyncDisposable objLocker = await objLock.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (++intLevel < 5)
                        {
                            if ((intLevel & 1) == 0)
                                await EnterWriteLockRecursivelyAsync(intLevel, token).ConfigureAwait(false);
                            else
                                await EnterUpgradeableReadLockRecursivelyAsync(intLevel, token).ConfigureAwait(false);
                        }
                        else
                        {
                            await EnterReadLockRecursivelyAsync(intLevel, token).ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
        }

        [TestMethod, Timeout(10 * 1000, CooperativeCancellation = true)]
        public void AsyncFriendlyReaderWriterLock_LocksCanCancel()
        {
            using (AsyncFriendlyReaderWriterLock objLock = new AsyncFriendlyReaderWriterLock())
            {
                int intStage = 0;
                Parallel.Invoke(
                    () =>
                    {
                        using (objLock.EnterWriteLock(TestContext.CancellationToken))
                        {
                            Interlocked.Increment(ref intStage); // Signal the other thread that the write lock has been acquired
                            while (intStage < 2) // Wait for other thread to signal this lock should be released
                                Utils.SafeSleep(TestContext.CancellationToken);
                        }
                    },
                    () =>
                    {
                        while (intStage == 0) // Wait for write lock to be acquired in the other thread
                            Utils.SafeSleep(TestContext.CancellationToken);
                        try
                        {
                            // Try to wait for and then cancel out of various locks
                            using (CancellationTokenSource objSource = new CancellationTokenSource(100))
                            {
                                try
                                {
                                    using (objLock.EnterReadLock(objSource.Token))
                                        Assert.Fail("Acquired a lock we weren't supposed to.");
                                }
                                catch (OperationCanceledException)
                                {
                                    // Swallow this, this is intended behavior
                                }
                            }
                            TestContext.CancellationToken.ThrowIfCancellationRequested();
                            using (CancellationTokenSource objSource = new CancellationTokenSource(100))
                            {
                                try
                                {
                                    using (objLock.EnterWriteLock(objSource.Token))
                                        Assert.Fail("Acquired a lock we weren't supposed to.");
                                }
                                catch (OperationCanceledException)
                                {
                                    // Swallow this, this is intended behavior
                                }
                            }
                            TestContext.CancellationToken.ThrowIfCancellationRequested();
                            using (CancellationTokenSource objSource = new CancellationTokenSource(100))
                            {
                                try
                                {
                                    using (objLock.EnterUpgradeableReadLock(objSource.Token))
                                        Assert.Fail("Acquired a lock we weren't supposed to.");
                                }
                                catch (OperationCanceledException)
                                {
                                    // Swallow this, this is intended behavior
                                }
                            }
                            // Same, but async versions
                            Utils.SafelyRunSynchronously(async t =>
                            {
                                t.ThrowIfCancellationRequested();
                                using (CancellationTokenSource objSource = new CancellationTokenSource(100))
                                {
                                    IAsyncDisposable objLocker = await objLock.EnterReadLockAsync(objSource.Token).ConfigureAwait(false);
                                    try
                                    {
                                        objSource.Token.ThrowIfCancellationRequested();
                                        Assert.Fail("Acquired a lock we weren't supposed to.");
                                    }
                                    catch (OperationCanceledException)
                                    {
                                        // Swallow this, this is intended behavior
                                    }
                                    finally
                                    {
                                        await objLocker.DisposeAsync().ConfigureAwait(false);
                                    }
                                }
                                t.ThrowIfCancellationRequested();
                                using (CancellationTokenSource objSource = new CancellationTokenSource(100))
                                {
                                    IAsyncDisposable objLocker = await objLock.EnterWriteLockAsync(objSource.Token).ConfigureAwait(false);
                                    try
                                    {
                                        objSource.Token.ThrowIfCancellationRequested();
                                        Assert.Fail("Acquired a lock we weren't supposed to.");
                                    }
                                    catch (OperationCanceledException)
                                    {
                                        // Swallow this, this is intended behavior
                                    }
                                    finally
                                    {
                                        await objLocker.DisposeAsync().ConfigureAwait(false);
                                    }
                                }
                                t.ThrowIfCancellationRequested();
                                using (CancellationTokenSource objSource = new CancellationTokenSource(100))
                                {
                                    IAsyncDisposable objLocker = await objLock.EnterUpgradeableReadLockAsync(objSource.Token).ConfigureAwait(false);
                                    try
                                    {
                                        objSource.Token.ThrowIfCancellationRequested();
                                        Assert.Fail("Acquired a lock we weren't supposed to.");
                                    }
                                    catch (OperationCanceledException)
                                    {
                                        // Swallow this, this is intended behavior
                                    }
                                    finally
                                    {
                                        await objLocker.DisposeAsync().ConfigureAwait(false);
                                    }
                                }
                            }, TestContext.CancellationToken);
                        }
                        finally
                        {
                            Interlocked.Increment(ref intStage); // We are done with our tests on this thread, signal the other thread to release its lock
                        }
                    });
            }
        }
    }
}
