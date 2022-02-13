using System.Threading;

namespace Chummer
{
    public interface IHasLockObject
    {
        ReaderWriterLockSlim LockObject { get; }
    }
}
