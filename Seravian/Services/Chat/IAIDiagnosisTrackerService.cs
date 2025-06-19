using System.Collections.Concurrent;

public interface IAIDiagnosisTrackerService
{
    void TryStartDiagnosing(Guid chatId);
    void MarkDiagnosisComplete(Guid chatId);
    bool IsDiagnosing(Guid chatId);
}

public class AIDiagnosisTrackerService : IAIDiagnosisTrackerService
{
    private readonly ConcurrentDictionary<Guid, bool> _statusMap = new();

    // Try to mark response as started (returns false if already in progress)
    public void TryStartDiagnosing(Guid chatId)
    {
        _statusMap[chatId] = true;
    }

    // Mark the response as completed
    public void MarkDiagnosisComplete(Guid chatId)
    {
        _statusMap.TryRemove(chatId, out _);
    }

    // Check if response is in progress
    public bool IsDiagnosing(Guid chatId)
    {
        return _statusMap.ContainsKey(chatId);
    }
}
