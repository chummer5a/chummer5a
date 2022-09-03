using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Chummer
{
    public interface IAsyncDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IAsyncCollection<KeyValuePair<TKey, TValue>>
    {
        ValueTask<ICollection<TKey>> GetKeysAsync(CancellationToken token = default);
        ValueTask<ICollection<TValue>> GetValuesAsync(CancellationToken token = default);
        ValueTask<TValue> GetValueAtAsync(TKey key, CancellationToken token = default);
        ValueTask SetValueAtAsync(TKey key, TValue value, CancellationToken token = default);
        /// <inheritdoc cref="IDictionary{TKey, TValue}.ContainsKey" />
        ValueTask<bool> ContainsKeyAsync(TKey key, CancellationToken token = default);
        ValueTask AddAsync(TKey key, TValue value, CancellationToken token = default);
        ValueTask<bool> RemoveAsync(TKey key, CancellationToken token = default);
        ValueTask<Tuple<bool, TValue>> TryGetValueAsync(TKey key, CancellationToken token = default);
    }
}
