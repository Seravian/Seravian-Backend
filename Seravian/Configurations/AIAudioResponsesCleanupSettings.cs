public class AIAudioResponsesCleanupSettings
{
    public string AudioFolder { get; set; } = default!;
    public int CleanupCutoffMinutes { get; set; } = 5;
    public int DelayMinutes { get; set; } = 1; // How often to check for cleanup
}
