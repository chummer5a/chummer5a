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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Chummer.Annotations;

namespace Chummer
{
    public static class ConcurrentDictionaryExtensions
    {
        public static Task<TValue> GetOrAddAsync<TKey, TValue>(
            [NotNull] this ConcurrentDictionary<TKey, TValue> dicThis, TKey key,
            Func<TKey, Task<TValue>> valueFactory, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<TValue>(token);
            return dicThis.TryGetValue(key, out TValue objReturn) ? Task.FromResult(objReturn) : Inner();

            async Task<TValue> Inner()
            {
                TValue objNewValue = await valueFactory.Invoke(key).ConfigureAwait(false);
                TValue objInnerReturn = dicThis.GetOrAdd(key, objNewValue);
                if (!ReferenceEquals(objInnerReturn, objNewValue))
                {
                    if (objNewValue is IAsyncDisposable objDisposeAsync)
                        await objDisposeAsync.DisposeAsync().ConfigureAwait(false);
                    else if (objNewValue is IDisposable objDispose)
                        objDispose.Dispose();
                }
                return objInnerReturn;
            }
        }

        public static Task<TValue> GetOrAddAsync<TKey, TValue>(
            [NotNull] this ConcurrentDictionary<TKey, TValue> dicThis, TKey key,
            Task<TValue> value, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<TValue>(token);
            return dicThis.TryGetValue(key, out TValue objReturn) ? Task.FromResult(objReturn) : Inner();

            async Task<TValue> Inner()
            {
                TValue objNewValue = await value.ConfigureAwait(false);
                TValue objInnerReturn = dicThis.GetOrAdd(key, objNewValue);
                if (!ReferenceEquals(objInnerReturn, objNewValue))
                {
                    if (objNewValue is IAsyncDisposable objDisposeAsync)
                        await objDisposeAsync.DisposeAsync().ConfigureAwait(false);
                    else if (objNewValue is IDisposable objDispose)
                        objDispose.Dispose();
                }
                return objInnerReturn;
            }
        }

        public static Task<TValue> GetOrAddAsync<TKey, TArg, TValue>(
            [NotNull] this ConcurrentDictionary<TKey, TValue> dicThis, TKey key,
            Func<TKey, TArg, Task<TValue>> valueFactory, TArg factoryArgument, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<TValue>(token);
            return dicThis.TryGetValue(key, out TValue objReturn) ? Task.FromResult(objReturn) : Inner();

            async Task<TValue> Inner()
            {
                TValue objNewValue = await valueFactory.Invoke(key, factoryArgument).ConfigureAwait(false);
                TValue objInnerReturn = dicThis.GetOrAdd(key, objNewValue);
                if (!ReferenceEquals(objInnerReturn, objNewValue))
                {
                    if (objNewValue is IAsyncDisposable objDisposeAsync)
                        await objDisposeAsync.DisposeAsync().ConfigureAwait(false);
                    else if (objNewValue is IDisposable objDispose)
                        objDispose.Dispose();
                }
                return objInnerReturn;
            }
        }

        public static Task<TValue> GetOrAddAsync<TKey, TArg, TValue>(
            [NotNull] this ConcurrentDictionary<TKey, TValue> dicThis, TKey key,
            Func<TKey, TArg, Task<TValue>> valueFactory, Task<TArg> factoryArgument, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<TValue>(token);
            return dicThis.TryGetValue(key, out TValue objReturn) ? Task.FromResult(objReturn) : Inner();

            async Task<TValue> Inner()
            {
                TValue objNewValue = await valueFactory.Invoke(key, await factoryArgument.ConfigureAwait(false))
                    .ConfigureAwait(false);
                TValue objInnerReturn = dicThis.GetOrAdd(key, objNewValue);
                if (!ReferenceEquals(objInnerReturn, objNewValue))
                {
                    if (objNewValue is IAsyncDisposable objDisposeAsync)
                        await objDisposeAsync.DisposeAsync().ConfigureAwait(false);
                    else if (objNewValue is IDisposable objDispose)
                        objDispose.Dispose();
                }
                return objInnerReturn;
            }
        }

        public static async Task<TValue> AddOrUpdateAsync<TKey, TArg, TValue>([NotNull] this ConcurrentDictionary<TKey, TValue> dicThis, TKey key, Func<TKey, TArg, Task<TValue>> addValueFactory,
                                                        Func<TKey, TValue, TArg, TValue> updateValueFactory, TArg factoryArgument, CancellationToken token = default)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                TValue objReturn;
                TValue objExistingValue;
                while (dicThis.TryGetValue(key, out objExistingValue))
                {
                    objReturn = updateValueFactory(key, objExistingValue, factoryArgument);
                    token.ThrowIfCancellationRequested();
                    if (dicThis.TryUpdate(key, objReturn, objExistingValue))
                        return objReturn;
                }

                objReturn = await addValueFactory(key, factoryArgument).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                objExistingValue = dicThis.GetOrAdd(key, objReturn);
                if (!ReferenceEquals(objExistingValue, objReturn))
                {
                    objReturn = updateValueFactory(key, objExistingValue, factoryArgument);
                    token.ThrowIfCancellationRequested();
                    if (dicThis.TryUpdate(key, objReturn, objExistingValue))
                        return objReturn;
                }
                else
                    return objReturn;
            }
        }

        public static async Task<TValue> AddOrUpdateAsync<TKey, TArg, TValue>([NotNull] this ConcurrentDictionary<TKey, TValue> dicThis, TKey key, Func<TKey, TArg, TValue> addValueFactory,
                                                        Func<TKey, TValue, TArg, Task<TValue>> updateValueFactory, TArg factoryArgument, CancellationToken token = default)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                TValue objReturn;
                TValue objExistingValue;
                while (dicThis.TryGetValue(key, out objExistingValue))
                {
                    objReturn = await updateValueFactory(key, objExistingValue, factoryArgument).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (dicThis.TryUpdate(key, objReturn, objExistingValue))
                        return objReturn;
                }

                objReturn = addValueFactory(key, factoryArgument);
                token.ThrowIfCancellationRequested();
                objExistingValue = dicThis.GetOrAdd(key, objReturn);
                if (!ReferenceEquals(objExistingValue, objReturn))
                {
                    objReturn = await updateValueFactory(key, objExistingValue, factoryArgument).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (dicThis.TryUpdate(key, objReturn, objExistingValue))
                        return objReturn;
                }
                else
                    return objReturn;
            }
        }

        public static async Task<TValue> AddOrUpdateAsync<TKey, TArg, TValue>([NotNull] this ConcurrentDictionary<TKey, TValue> dicThis, TKey key, Func<TKey, TArg, Task<TValue>> addValueFactory,
            Func<TKey, TValue, TArg, Task<TValue>> updateValueFactory, TArg factoryArgument, CancellationToken token = default)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                TValue objReturn;
                TValue objExistingValue;
                while (dicThis.TryGetValue(key, out objExistingValue))
                {
                    objReturn = await updateValueFactory(key, objExistingValue, factoryArgument).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (dicThis.TryUpdate(key, objReturn, objExistingValue))
                        return objReturn;
                }

                objReturn = await addValueFactory(key, factoryArgument).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                objExistingValue = dicThis.GetOrAdd(key, objReturn);
                if (!ReferenceEquals(objExistingValue, objReturn))
                {
                    objReturn = await updateValueFactory(key, objExistingValue, factoryArgument).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (dicThis.TryUpdate(key, objReturn, objExistingValue))
                        return objReturn;
                }
                else
                    return objReturn;
            }
        }

        public static async Task<TValue> AddOrUpdateAsync<TKey, TArg, TValue>([NotNull] this ConcurrentDictionary<TKey, TValue> dicThis, TKey key, TValue addValue,
            Func<TKey, TValue, TArg, Task<TValue>> updateValueFactory, TArg factoryArgument, CancellationToken token = default)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                TValue objReturn;
                TValue objExistingValue;
                while (dicThis.TryGetValue(key, out objExistingValue))
                {
                    objReturn = await updateValueFactory(key, objExistingValue, factoryArgument).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (dicThis.TryUpdate(key, objReturn, objExistingValue))
                        return objReturn;
                }

                objReturn = addValue;
                token.ThrowIfCancellationRequested();
                objExistingValue = dicThis.GetOrAdd(key, objReturn);
                if (!ReferenceEquals(objExistingValue, objReturn))
                {
                    objReturn = await updateValueFactory(key, objExistingValue, factoryArgument).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (dicThis.TryUpdate(key, objReturn, objExistingValue))
                        return objReturn;
                }
                else
                    return objReturn;
            }
        }

        public static async Task<TValue> AddOrUpdateAsync<TKey, TArg, TValue>([NotNull] this ConcurrentDictionary<TKey, TValue> dicThis, TKey key, Task<TValue> addValue,
            Func<TKey, TValue, TArg, Task<TValue>> updateValueFactory, TArg factoryArgument, CancellationToken token = default)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                TValue objReturn;
                TValue objExistingValue;
                while (dicThis.TryGetValue(key, out objExistingValue))
                {
                    objReturn = await updateValueFactory(key, objExistingValue, factoryArgument).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (dicThis.TryUpdate(key, objReturn, objExistingValue))
                        return objReturn;
                }

                objReturn = await addValue.ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                objExistingValue = dicThis.GetOrAdd(key, objReturn);
                if (!ReferenceEquals(objExistingValue, objReturn))
                {
                    objReturn = await updateValueFactory(key, objExistingValue, factoryArgument).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (dicThis.TryUpdate(key, objReturn, objExistingValue))
                        return objReturn;
                }
                else
                    return objReturn;
            }
        }

        public static async Task<TValue> AddOrUpdateAsync<TKey, TValue>([NotNull] this ConcurrentDictionary<TKey, TValue> dicThis, TKey key, Func<TKey, Task<TValue>> addValueFactory,
                                                        Func<TKey, TValue, TValue> updateValueFactory, CancellationToken token = default)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                TValue objReturn;
                TValue objExistingValue;
                while (dicThis.TryGetValue(key, out objExistingValue))
                {
                    objReturn = updateValueFactory(key, objExistingValue);
                    token.ThrowIfCancellationRequested();
                    if (dicThis.TryUpdate(key, objReturn, objExistingValue))
                        return objReturn;
                }

                objReturn = await addValueFactory(key).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                objExistingValue = dicThis.GetOrAdd(key, objReturn);
                if (!ReferenceEquals(objExistingValue, objReturn))
                {
                    objReturn = updateValueFactory(key, objExistingValue);
                    token.ThrowIfCancellationRequested();
                    if (dicThis.TryUpdate(key, objReturn, objExistingValue))
                        return objReturn;
                }
                else
                    return objReturn;
            }
        }

        public static async Task<TValue> AddOrUpdateAsync<TKey, TValue>([NotNull] this ConcurrentDictionary<TKey, TValue> dicThis, TKey key, Func<TKey, TValue> addValueFactory,
                                                        Func<TKey, TValue, Task<TValue>> updateValueFactory, CancellationToken token = default)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                TValue objReturn;
                TValue objExistingValue;
                while (dicThis.TryGetValue(key, out objExistingValue))
                {
                    objReturn = await updateValueFactory(key, objExistingValue).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (dicThis.TryUpdate(key, objReturn, objExistingValue))
                        return objReturn;
                }

                objReturn = addValueFactory(key);
                token.ThrowIfCancellationRequested();
                objExistingValue = dicThis.GetOrAdd(key, objReturn);
                if (!ReferenceEquals(objExistingValue, objReturn))
                {
                    objReturn = await updateValueFactory(key, objExistingValue).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (dicThis.TryUpdate(key, objReturn, objExistingValue))
                        return objReturn;
                }
                else
                    return objReturn;
            }
        }

        public static async Task<TValue> AddOrUpdateAsync<TKey, TValue>([NotNull] this ConcurrentDictionary<TKey, TValue> dicThis, TKey key, Func<TKey, Task<TValue>> addValueFactory,
            Func<TKey, TValue, Task<TValue>> updateValueFactory, CancellationToken token = default)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                TValue objReturn;
                TValue objExistingValue;
                while (dicThis.TryGetValue(key, out objExistingValue))
                {
                    objReturn = await updateValueFactory(key, objExistingValue).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (dicThis.TryUpdate(key, objReturn, objExistingValue))
                        return objReturn;
                }

                objReturn = await addValueFactory(key).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                objExistingValue = dicThis.GetOrAdd(key, objReturn);
                if (!ReferenceEquals(objExistingValue, objReturn))
                {
                    objReturn = await updateValueFactory(key, objExistingValue).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (dicThis.TryUpdate(key, objReturn, objExistingValue))
                        return objReturn;
                }
                else
                    return objReturn;
            }
        }

        public static async Task<TValue> AddOrUpdateAsync<TKey, TValue>([NotNull] this ConcurrentDictionary<TKey, TValue> dicThis, TKey key, TValue addValue,
            Func<TKey, TValue, Task<TValue>> updateValueFactory, CancellationToken token = default)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                TValue objReturn;
                TValue objExistingValue;
                while (dicThis.TryGetValue(key, out objExistingValue))
                {
                    objReturn = await updateValueFactory(key, objExistingValue).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (dicThis.TryUpdate(key, objReturn, objExistingValue))
                        return objReturn;
                }

                objReturn = addValue;
                token.ThrowIfCancellationRequested();
                objExistingValue = dicThis.GetOrAdd(key, objReturn);
                if (!ReferenceEquals(objExistingValue, objReturn))
                {
                    objReturn = await updateValueFactory(key, objExistingValue).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (dicThis.TryUpdate(key, objReturn, objExistingValue))
                        return objReturn;
                }
                else
                    return objReturn;
            }
        }

        public static async Task<TValue> AddOrUpdateAsync<TKey, TValue>([NotNull] this ConcurrentDictionary<TKey, TValue> dicThis, TKey key, Task<TValue> addValue,
            Func<TKey, TValue, Task<TValue>> updateValueFactory, CancellationToken token = default)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                TValue objReturn;
                TValue objExistingValue;
                while (dicThis.TryGetValue(key, out objExistingValue))
                {
                    objReturn = await updateValueFactory(key, objExistingValue).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (dicThis.TryUpdate(key, objReturn, objExistingValue))
                        return objReturn;
                }

                objReturn = await addValue.ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                objExistingValue = dicThis.GetOrAdd(key, objReturn);
                if (!ReferenceEquals(objExistingValue, objReturn))
                {
                    objReturn = await updateValueFactory(key, objExistingValue).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (dicThis.TryUpdate(key, objReturn, objExistingValue))
                        return objReturn;
                }
                else
                    return objReturn;
            }
        }

        /// <summary>
        /// Syntactic sugar for getting the keys of a ConcurrentDictionary in a new list in a way where the original dictionary's keys are locked against changes until the list is complete.
        /// </summary>
        public static List<TKey> GetKeysToListSafe<TKey, TValue>([NotNull] this ConcurrentDictionary<TKey, TValue> dicThis)
        {
            List<TKey> lstReturn = new List<TKey>(dicThis.Count);
            foreach (KeyValuePair<TKey, TValue> kvpLoop in dicThis)
            {
                lstReturn.Add(kvpLoop.Key);
            }
            return lstReturn;
        }

        /// <summary>
        /// Syntactic sugar for getting the values of a ConcurrentDictionary in a new list in a way where the original dictionary's values are locked against changes until the list is complete.
        /// </summary>
        public static List<TValue> GetValuesToListSafe<TKey, TValue>([NotNull] this ConcurrentDictionary<TKey, TValue> dicThis)
        {
            List<TValue> lstReturn = new List<TValue>(dicThis.Count);
            foreach (KeyValuePair<TKey, TValue> kvpLoop in dicThis)
            {
                lstReturn.Add(kvpLoop.Value);
            }
            return lstReturn;
        }
    }
}
