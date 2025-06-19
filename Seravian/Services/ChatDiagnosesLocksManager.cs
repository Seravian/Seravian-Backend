using System.Collections.Concurrent;

public class ChatDiagnosesLocksManager
{
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = [];

    public async Task<bool> TryLock(Guid chatId)
    {
        var sem = _locks.GetOrAdd(chatId, _ => new SemaphoreSlim(1, 1));

        return await sem.WaitAsync(0); // no wait — instant lock check
    }

    public void Release(Guid chatId)
    {
        if (_locks.TryGetValue(chatId, out var sem))
        {
            sem.Release();
        }
    }
}
