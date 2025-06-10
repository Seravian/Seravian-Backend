using System.Collections.Concurrent;

public class DoctorsVerificationRequestsAttachmentFilesAccessLockingManager
{
    private readonly ConcurrentDictionary<int, ReaderWriterLockSlim> _locks = new();

    public void EnterRead(int requestId)
    {
        var rwLock = _locks.GetOrAdd(requestId, _ => new ReaderWriterLockSlim());
        rwLock.EnterReadLock();
    }

    public void ExitRead(int requestId)
    {
        if (_locks.TryGetValue(requestId, out var rwLock))
        {
            rwLock.ExitReadLock();
        }
    }

    public void EnterWrite(int requestId)
    {
        var rwLock = _locks.GetOrAdd(requestId, _ => new ReaderWriterLockSlim());
        rwLock.EnterWriteLock();
    }

    public void ExitWrite(int requestId)
    {
        if (_locks.TryGetValue(requestId, out var rwLock))
        {
            rwLock.ExitWriteLock();
        }
    }
}
