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
    /// <summary>
    /// A slimmed-down and simplified version of ObjectPool that should have fewer weird behaviors than the Microsoft-provided ObjectPool.
    /// Unlike Microsoft-provided ObjectPool, the pool is set up in a way that always tracks garbage collections and should guarantee that it's impossible
    /// to return the same object twice unless you really go out of your way to make that happen.
    /// </summary>
    /// <typeparam name="T">A disposable class to use as the type of object being pooled.</typeparam>
    public sealed class SafeObjectPool<T> where T : class
    {
        private readonly T[] _aobjSwappablePool;
        private readonly T[] _aobjStoragePool; // Storage pool used only to make sure that we only return stuff that is from the pool originally
        private readonly Func<T> _funcConstructor;
        private readonly Action<T> _actionRunOnReturn;

        /// <summary>
        /// Create a simplified version of an object pool.
        /// NOTE: DO NOT USE THIS FOR DISPOSABLE OBJECTS!
        /// This constructor also initializes all objects in its pool when it is called.
        /// </summary>
        /// <param name="funcConstructor">Delegate to the constructor to be used for all objects in the pool.</param>
        /// <param name="actionRunOnReturn">Optional delegate to function to run when an object is returned to the pool, including if it's forcibly returned should the pool be disposed without it.</param>
        public SafeObjectPool(Func<T> funcConstructor, Action<T> actionRunOnReturn = null) : this(Math.Max(Utils.MaxParallelBatchSize, 16), funcConstructor, actionRunOnReturn)
        {
        }

        /// <summary>
        /// Create a simplified version of an object pool.
        /// NOTE: DO NOT USE THIS FOR DISPOSABLE OBJECTS!
        /// This constructor also initializes all objects in its pool when it is called.
        /// </summary>
        /// <param name="intPoolSize">Size of the pool to use. Remains static for the lifetime of the pool.</param>
        /// <param name="funcConstructor">Delegate to the constructor to be used for all objects in the pool.</param>
        /// <param name="actionRunOnReturn">Optional delegate to function to run when an object is returned to the pool, including if it's forcibly returned should the pool be disposed without it.</param>
        public SafeObjectPool(int intPoolSize, Func<T> funcConstructor, Action<T> actionRunOnReturn = null)
        {
            _aobjSwappablePool = new T[intPoolSize];
            _aobjStoragePool = new T[intPoolSize];
            _funcConstructor = funcConstructor;
            _actionRunOnReturn = actionRunOnReturn;
            for (int i = 0; i < intPoolSize; ++i)
            {
                T objLoop = _funcConstructor();
                _aobjSwappablePool[i] = objLoop;
                _aobjStoragePool[i] = objLoop;
            }
        }

        /// <summary>
        /// Gets an object from the pool. If possible, try to save this and return it once it is no longer used to make the most use of the pool.
        /// </summary>
        public T Get()
        {
            for (int i = 0; i < _aobjSwappablePool.Length; ++i)
            {
                T objLoop = Interlocked.Exchange(ref _aobjSwappablePool[i], null);
                if (objLoop != null)
                    return objLoop;
            }
            return _funcConstructor();
        }

        /// <summary>
        /// Returns an object to the pool and sets the pointer that holds it to null as a precautionary measure.
        /// </summary>
        public void Return(ref T objToReturn)
        {
            // Immediately interlocked exchange the object we're returning to make sure we don't somehow hold onto it
            T objLocal = Interlocked.Exchange(ref objToReturn, null);
            if (_actionRunOnReturn != null)
            {
                try
                {
                    _actionRunOnReturn.Invoke(objLocal);
                }
                catch (Exception)
                {
                    // Exchange back if we run into an exception
                    Interlocked.CompareExchange(ref objToReturn, objLocal, null);
                    throw;
                }
            }
            // Check that we're return an item that was originally in the pool
            for (int i = 0; i < _aobjStoragePool.Length; ++i)
            {
                if (ReferenceEquals(objLocal, _aobjStoragePool[i]))
                {
                    if (Interlocked.CompareExchange(ref _aobjSwappablePool[i], objLocal, null) != null)
                    {
                        // We are somehow returning an item that was already returned?
                        Utils.BreakIfDebug();
                    }
                    return;
                }
            }
        }
    }
}
