using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Chummer
{
    public interface IAsyncCollection<T> : ICollection<T>, IAsyncEnumerable<T>
    {
        ValueTask<int> GetCountAsync(CancellationToken token = default);
        ValueTask AddAsync(T item, CancellationToken token = default);
        ValueTask ClearAsync(CancellationToken token = default);
        ValueTask<bool> ContainsAsync(T item, CancellationToken token = default);
        ValueTask CopyToAsync(T[] array, int index, CancellationToken token = default);
        ValueTask<bool> RemoveAsync(T item, CancellationToken token = default);
    }

    public static class AsyncCollectionExtensions
    {
        public static async Task AddRangeAsync<T>(this IAsyncCollection<T> lstCollection, IEnumerable<T> lstToAdd, CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            foreach (T objItem in lstToAdd)
                await lstCollection.AddAsync(objItem, token);
        }

        public static async Task AddRangeAsync<T>(this IAsyncCollection<T> lstCollection, IAsyncEnumerable<T> lstToAdd, CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            using (IEnumerator<T> objEnumerator = await lstToAdd.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    await lstCollection.AddAsync(objEnumerator.Current, token);
                }
            }
        }
    }
}
