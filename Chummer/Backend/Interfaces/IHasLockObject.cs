using System;
using System.Threading;

namespace Chummer
{
    public interface IHasLockObject : IDisposable
    {
        ReaderWriterLockSlim LockObject { get; }
    }
}
