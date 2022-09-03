using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Chummer
{
    public interface IAsyncReadOnlyList<T> : IAsyncReadOnlyCollection<T>, IReadOnlyList<T>
    {
        ValueTask<T> GetValueAtAsync(int index, CancellationToken token = default);
    }
}
