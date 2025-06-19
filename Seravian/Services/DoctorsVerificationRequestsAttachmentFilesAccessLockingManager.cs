using System.Collections.Concurrent;
using Nito.AsyncEx;

public class DoctorsVerificationRequestsAttachmentFilesAccessLockingManager
{
    private readonly ConcurrentDictionary<long, AsyncReaderWriterLock> _locks = [];

    public async Task<IDisposable> EnterReadAsync(long requestId)
    {
        var rwLock = _locks.GetOrAdd(requestId, _ => new AsyncReaderWriterLock());
        return await rwLock.ReaderLockAsync();
    }

    public async Task<IDisposable> EnterWriteAsync(long requestId)
    {
        var rwLock = _locks.GetOrAdd(requestId, _ => new AsyncReaderWriterLock());
        return await rwLock.WriterLockAsync();
    }
}
