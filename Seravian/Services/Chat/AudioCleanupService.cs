using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class AIAudioResponsesCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AIAudioResponsesCleanupService> _logger;
    private readonly AIAudioResponsesCleanupSettings _settings;

    public AIAudioResponsesCleanupService(
        IServiceScopeFactory scopeFactory,
        IOptions<AIAudioResponsesCleanupSettings> options,
        ILogger<AIAudioResponsesCleanupService> logger
    )
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _settings = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var cutoff = DateTime.UtcNow.AddMinutes(-_settings.CleanupCutoffMinutes);

                var oldAIResponses = await dbContext
                    .ChatsMessages.Where(m => m.IsAI && m.TimestampUtc <= cutoff)
                    .Select(x => new { AudioId = x.Id, ChatId = x.ChatId })
                    .ToListAsync(stoppingToken);

                foreach (var oldAudioResponse in oldAIResponses)
                {
                    // Construct path: {AudioFolder}/{ChatId}/{MessageId}.wav
                    var audioFilePath = Path.Combine(
                        _settings.AudioFolder,
                        oldAudioResponse.ChatId.ToString(),
                        $"{oldAudioResponse.AudioId}.wav"
                    );

                    if (File.Exists(audioFilePath))
                    {
                        try
                        {
                            File.Delete(audioFilePath);
                            _logger.LogInformation($"Deleted AI audio file: {audioFilePath}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error deleting audio file: {audioFilePath}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during AI audio cleanup.");
            }

            await Task.Delay(TimeSpan.FromMinutes(_settings.DelayMinutes), stoppingToken);
        }
    }
}
