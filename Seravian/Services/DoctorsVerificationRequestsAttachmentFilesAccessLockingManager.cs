using System.Collections.Concurrent;
using Nito.AsyncEx;

public class DoctorsVerificationRequestsAttachmentFilesAccessLockingManager
{
    private readonly ConcurrentDictionary<int, AsyncReaderWriterLock> _locks = [];

    public async Task<IDisposable> EnterReadAsync(int requestId)
    {
        var rwLock = _locks.GetOrAdd(requestId, _ => new AsyncReaderWriterLock());
        return await rwLock.ReaderLockAsync();
    }

    public async Task<IDisposable> EnterWriteAsync(int requestId)
    {
        var rwLock = _locks.GetOrAdd(requestId, _ => new AsyncReaderWriterLock());
        return await rwLock.WriterLockAsync();
    }
}
