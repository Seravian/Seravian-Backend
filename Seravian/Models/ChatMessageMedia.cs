public class ChatMessageMedia
{
    public long MessageId { get; set; }

    public MediaType MediaType { get; set; }
    public string FileName { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public string? MimeType { get; set; } = null!;
    public long? FileSizeInBytes { get; set; }

    public string? Transcription { get; set; }
    public string? SEREmotionAnalysis { get; set; }
    public string? FaceAnalysis { get; set; }
    public string? CombinedAnalysisResult { get; set; }

    public virtual ChatMessage Message { get; set; }
}
