using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Chummer
{
    public interface IAsyncSet<T> : IAsyncCollection<T>, ISet<T>
    {
        new ValueTask<bool> AddAsync(T item, CancellationToken token = default);
        ValueTask UnionWithAsync(IEnumerable<T> other, CancellationToken token = default);
        ValueTask IntersectWithAsync(IEnumerable<T> other, CancellationToken token = default);
        ValueTask ExceptWithAsync(IEnumerable<T> other, CancellationToken token = default);
        ValueTask SymmetricExceptWithAsync(IEnumerable<T> other, CancellationToken token = default);
        ValueTask<bool> IsSubsetOfAsync(IEnumerable<T> other, CancellationToken token = default);
        ValueTask<bool> IsSupersetOfAsync(IEnumerable<T> other, CancellationToken token = default);
        ValueTask<bool> IsProperSupersetOfAsync(IEnumerable<T> other, CancellationToken token = default);
        ValueTask<bool> IsProperSubsetOfAsync(IEnumerable<T> other, CancellationToken token = default);
        ValueTask<bool> OverlapsAsync(IEnumerable<T> other, CancellationToken token = default);
        ValueTask<bool> SetEqualsAsync(IEnumerable<T> other, CancellationToken token = default);
    }
}
