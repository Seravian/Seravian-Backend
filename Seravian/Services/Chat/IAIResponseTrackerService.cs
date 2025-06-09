using System.Collections.Concurrent;

public interface IAIResponseTrackerService
{
    void TryStartResponse(Guid chatId);
    void MarkResponseComplete(Guid chatId);
    bool IsResponding(Guid chatId);
}

public class AIResponseTrackerService : IAIResponseTrackerService
{
    private readonly ConcurrentDictionary<Guid, bool> _statusMap = new();

    // Try to mark response as started (returns false if already in progress)
    public void TryStartResponse(Guid chatId)
    {
        _statusMap[chatId] = true;
    }

    // Mark the response as completed
    public void MarkResponseComplete(Guid chatId)
    {
        _statusMap.TryRemove(chatId, out _);
    }

    // Check if response is in progress
    public bool IsResponding(Guid chatId)
    {
        return _statusMap.ContainsKey(chatId);
    }
}
