public class ChatVoiceAnalysis
{
    public long MessageId { get; set; }
    public string? Transcription { get; set; }
    public string? SEREmotionAnalysis { get; set; }
    public string? CombinedAnalysisResult { get; set; }
    public virtual ChatMessage Message { get; set; }
}
