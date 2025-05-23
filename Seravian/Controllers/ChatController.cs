using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Seravian.Hubs;
using TestAIModels;

[Route("[controller]")]
[ApiController]
[Authorize(Roles = "Patient")]
public class ChatController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly ChatProcessingManager _llmProcessingManager;
    private readonly IHubContext<ChatHub, IChatHubClient> _hubContext;
    private readonly AIAudioAnalyzingService _voiceAnalysisService;
    private readonly LLMService _llmService;
    private readonly TTSService _ttsService;
    private readonly IAudioService _audioService;
    private readonly IOptionsMonitor<AudioPathsSettings> _audioPathsSettings;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        ApplicationDbContext dbContext,
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        ChatProcessingManager llmProcessingManager,
        IHubContext<ChatHub, IChatHubClient> hubContext,
        AIAudioAnalyzingService voiceAnalysisService,
        LLMService llmService,
        TTSService ttsService,
        IAudioService audioService,
        IOptionsMonitor<AudioPathsSettings> audioPathsSettings,
        ILogger<ChatController> logger
    )
    {
        _dbContext = dbContext;
        _dbContextFactory = dbContextFactory;
        _llmProcessingManager = llmProcessingManager;
        _hubContext = hubContext;
        _voiceAnalysisService = voiceAnalysisService;
        _llmService = llmService;
        _ttsService = ttsService;
        _audioService = audioService;
        _audioPathsSettings = audioPathsSettings;
        _logger = logger;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateChatAsync(CreateChatRequestDto request)
    {
        if (request.Title?.Length > 50)
        {
            return BadRequest(
                new
                {
                    Errors = new List<string> { "Title length must be less than or equal to 50." },
                }
            );
        }
        var patientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.Parse(patientId),
            Title = request.Title,
            CreatedAtUtc = DateTime.UtcNow,
        };

        await _dbContext.Chats.AddAsync(chat);
        await _dbContext.SaveChangesAsync();

        return Ok(
            new CreateChatResponseDto
            {
                Id = chat.Id,
                Title = chat.Title,
                CreatedAtUtc = chat.CreatedAtUtc,
            }
        );
    }

    [HttpPut("update")]
    public async Task<IActionResult> ChatInfoUpdateAsync(UpdateChatInfoRequestDto request)
    {
        List<string> errors = [];
        #region validation
        if (request.Id == Guid.Empty || request.Id == null)
        {
            errors.Add("Chat ID is required.");
        }
        if (request.Title?.Length > 50)
        {
            errors.Add("Title length must be less than or equal to 50.");
        }
        if (errors.Count > 0)
        {
            return BadRequest(new { Errors = errors });
        }
        var patientId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        Chat? chat = await _dbContext
            .Chats.Where(c => c.Id == request.Id && c.PatientId == patientId && !c.IsDeleted)
            .FirstOrDefaultAsync();

        if (chat is null)
        {
            errors.Add("Chat not found.");
        }
        #endregion


        if (errors.Count > 0)
        {
            return BadRequest(new { Errors = errors });
        }

        chat.Title = request.Title;
        await _dbContext.SaveChangesAsync();

        return Ok(
            new ChatInfoUpdateResponseDto
            {
                Id = chat.Id,
                Title = chat.Title,
                CreatedAtUtc = chat.CreatedAtUtc,
            }
        );
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteChatAsync(DeleteChatRequestDto request)
    {
        #region validation
        if (request.Id == null || request.Id == Guid.Empty)
        {
            return BadRequest(new { Errors = new List<string> { "Chat ID is required." } });
        }

        var patientId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        Chat? chat = await _dbContext
            .Chats.Where(c => c.Id == request.Id && c.PatientId == patientId && !c.IsDeleted)
            .FirstOrDefaultAsync();

        if (chat is null)
        {
            return BadRequest(new { Errors = new List<string> { "Chat not found." } });
        }
        #endregion


        chat.IsDeleted = true;
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("get-chats")]
    public async Task<ActionResult<GetChatResponseDto>> GetChatsAsync()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var chats = await _dbContext
            .Chats.Where(c => c.PatientId == userId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAtUtc)
            .Select(c => new GetChatResponseDto
            {
                Id = c.Id,
                Title = c.Title,
                CreatedAt = c.CreatedAtUtc,
            })
            .ToListAsync();

        return Ok(chats);
    }

    [HttpGet("get-chat-messages")]
    public async Task<IActionResult> GetChatMessagesAsync(
        [FromQuery] GetChatMessagesRequestDto request
    )
    {
        if (request.Id == null || request.Id == Guid.Empty)
        {
            return BadRequest(new { Errors = new List<string> { "Chat ID is required." } });
        }
        var patientId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var chat = await _dbContext
            .Chats.Include(C => C.ChatMessages)
            .AsNoTracking()
            .Where(c => c.Id == request.Id && c.PatientId == patientId && !c.IsDeleted)
            .FirstOrDefaultAsync();

        if (chat is null)
        {
            return BadRequest(new { Errors = new List<string> { "Chat not found." } });
        }

        var response = new GetChatMessagesResponseDto
        {
            Id = chat.Id,
            Title = chat.Title,
            CreatedAt = chat.CreatedAtUtc,
            Messages = chat
                .ChatMessages.Where(m => !m.IsDeleted)
                .OrderBy(m => m.TimestampUtc)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    Content = m.Content,
                    TimestampUtc = m.TimestampUtc,
                    MessageType = m.MessageType,
                    IsAI = m.IsAI,
                })
                .ToList(),
        };

        return Ok(response);
    }

    [HttpGet("sync-messages")]
    public async Task<ActionResult<List<ChatMessageDto>>> SyncMessagesAsync(
        [FromQuery] SyncMessagesRequestDto request
    )
    {
        if (request.ChatId == null || request.ChatId == Guid.Empty)
        {
            return BadRequest(new { Errors = new List<string> { "Chat ID is required." } });
        }

        var patientId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var chat = await _dbContext
            .Chats.Include(C => C.ChatMessages)
            .AsNoTracking()
            .Where(c => c.Id == request.ChatId && c.PatientId == patientId && !c.IsDeleted)
            .FirstOrDefaultAsync();

        if (chat is null)
        {
            return BadRequest(new { Errors = new List<string> { "Chat not found." } });
        }
        Func<ChatMessage, bool> filter;

        if (request.LastMessageId is null)
            filter = m => !m.IsDeleted;
        else
            filter = m => m.Id > request.LastMessageId && !m.IsDeleted;

        var messages = chat
            .ChatMessages.Where(filter)
            .OrderBy(m => m.TimestampUtc)
            .Select(m => new ChatMessageDto
            {
                Id = m.Id,
                Content = m.Content,
                TimestampUtc = m.TimestampUtc,
                IsAI = m.IsAI,
                MessageType = m.MessageType,
            })
            .ToList();

        return Ok(messages);
    }

    [HttpGet("voice-mode-download-ai-voice")]
    public async Task<IActionResult> DownloadAIAudio([FromQuery] DownloadAIAudioRequestDto request)
    {
        var patientId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        var message = await _dbContext
            .ChatsMessages.Include(m => m.Chat)
            .AsNoTracking()
            .FirstOrDefaultAsync(m =>
                m.Id == request.AIAudioId
                && !m.IsDeleted
                && !m.Chat.IsDeleted
                && m.Chat.PatientId == patientId
                && m.IsAI
            );

        if (message is null)
            return NotFound("AI audio not found or inaccessible.");

        var filePath = Path.Combine(
            _audioPathsSettings.CurrentValue.AIOutputFolder,
            message.ChatId.ToString(),
            $"{request.AIAudioId}.wav"
        );

        try
        {
            if (!System.IO.File.Exists(filePath))
                return NotFound("AI audio not found or inaccessible.");

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(stream, "audio/wav", $"ai-response-{request.AIAudioId}.wav");
        }
        catch (Exception)
        {
            return NotFound("AI audio not found or inaccessible.");
        }
    }

    [RequestSizeLimit(20 * 1024 * 1024)]
    [HttpPost("voice-mode-upload-user-voice")]
    public async Task<IActionResult> UploadVoice(
        [FromForm] IFormFile voiceFile,
        [FromForm] Guid chatId
    )
    {
        var patientId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        if (
            !await _dbContext.Chats.AnyAsync(c =>
                c.Id == chatId && c.PatientId == patientId && !c.IsDeleted
            )
        )
        {
            return BadRequest(new { Errors = new List<string> { "Chat not found." } });
        }

        // Check if AI is busy processing for this chatId
        if (voiceFile is null || voiceFile.Length == 0)
            return BadRequest("Voice file is required.");

        // Allowed MIME types list (expand as needed)
        var allowedMimeTypes = new[]
        {
            "audio/wav",
            "audio/x-wav",
            "audio/webm",
            "audio/mpeg",
            "audio/mp3",
            "audio/ogg",
            "audio/flac",
            "audio/x-matroska", // e.g., webm sometimes reports this
            "video/webm", // some browsers send this for webm audio-only files
        };

        if (!allowedMimeTypes.Contains(voiceFile.ContentType))
            return BadRequest("Unsupported audio format.");

        if (!await _llmProcessingManager.TryLock(chatId))
        {
            // Lock not acquired = AI is still processing; reject new uploads
            return Conflict("AI response is still being generated for this chat. Please wait.");
        }

        try
        {
            var uploadsFolder = Path.Combine(
                _audioPathsSettings.CurrentValue.UserUploadFolder,
                chatId.ToString()
            );
            Directory.CreateDirectory(uploadsFolder);

            var tempPath = Path.Combine(
                uploadsFolder,
                $"{Guid.NewGuid()}{Path.GetExtension(voiceFile.FileName)}"
            );
            await using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await voiceFile.CopyToAsync(stream);
            }

            try
            {
                var wavPath = await _audioService.ValidateAndConvertToWavAsync(
                    tempPath,
                    uploadsFolder
                );

                _ = Task.Run(async () =>
                {
                    try
                    {
                        AudioAnalyzingResult analysisResult =
                            await _voiceAnalysisService.AnalyzeAudio(wavPath);

                        System.IO.File.Delete(wavPath);

                        var userChatMessage = new ChatMessage
                        {
                            ChatId = chatId,
                            Content = analysisResult.Transcription,
                            IsAI = false,
                            TimestampUtc = DateTime.UtcNow,
                            MessageType = MessageType.VoiceModeText,
                            VoiceAnalysis = new ChatVoiceAnalysis()
                            {
                                Transcription = analysisResult.Transcription,
                                SEREmotionAnalysis = analysisResult.DominantEmotion.ToString(),
                            },
                        };

                        using var dbContext = _dbContextFactory.CreateDbContext();

                        await dbContext.ChatsMessages.AddAsync(userChatMessage);
                        await dbContext.SaveChangesAsync();

                        await _hubContext
                            .Clients.Group(chatId.ToString())
                            .ReceiveClientRequestAsync(
                                new()
                                {
                                    Id = userChatMessage.Id,
                                    Message = userChatMessage.Content,
                                    TimestampUtc = userChatMessage.TimestampUtc,
                                    MessageType = MessageType.VoiceModeText,
                                    ChatId = chatId,
                                }
                            );

                        var formatLLMInput =
                            $"Respond accordingly: {analysisResult.Transcription}."
                            + $" Take note that the i'm feeling {analysisResult.DominantEmotion}.";

                        var llmResponse = await _llmService.SendMessageToLLMAsync(
                            formatLLMInput,
                            userChatMessage.Id,
                            chatId.ToString()
                        );

                        var ttsAudioBase64 = await _ttsService.GenerateVoiceFromText(llmResponse);

                        var outputFolder = Path.Combine(
                            _audioPathsSettings.CurrentValue.AIOutputFolder,
                            chatId.ToString()
                        );

                        var aiResponseChatMessage = new ChatMessage
                        {
                            ChatId = chatId,
                            Content = llmResponse,
                            IsAI = true,
                            TimestampUtc = DateTime.UtcNow,
                            MessageType = MessageType.VoiceModeText,
                        };

                        await dbContext.ChatsMessages.AddAsync(aiResponseChatMessage);
                        await dbContext.SaveChangesAsync();

                        Directory.CreateDirectory(outputFolder);

                        var aiAudioResponseFilePath = Path.Combine(
                            outputFolder,
                            $"{aiResponseChatMessage.Id}.wav"
                        );
                        byte[] audioBytes = Convert.FromBase64String(ttsAudioBase64);
                        await System.IO.File.WriteAllBytesAsync(
                            aiAudioResponseFilePath,
                            audioBytes
                        );

                        await _hubContext
                            .Clients.Group(chatId.ToString())
                            .NotifyAiAudioResponseReadyAsync(
                                new() { AIAudioId = aiResponseChatMessage.Id, ChatId = chatId }
                            );

                        await _hubContext
                            .Clients.Group(chatId.ToString())
                            .ReceiveAIResponseAsync(
                                new()
                                {
                                    Id = aiResponseChatMessage.Id,
                                    Message = aiResponseChatMessage.Content,
                                    TimestampUtc = aiResponseChatMessage.TimestampUtc,
                                    MessageType = MessageType.VoiceModeText,
                                    ChatId = chatId,
                                }
                            );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing voice file.");
                    }
                    finally
                    {
                        _llmProcessingManager.Release(chatId);
                    }
                });

                return Ok(
                    new { message = "Voice received and processing started.", path = wavPath }
                );
            }
            catch (Exception ex)
            {
                // Optionally delete temp file on failure
                if (System.IO.File.Exists(tempPath))
                    System.IO.File.Delete(tempPath);

                _llmProcessingManager.Release(chatId);
                return BadRequest("Failed to process voice file.");
            }
        }
        catch (Exception ex)
        {
            _llmProcessingManager.Release(chatId);
            return BadRequest(ex.Message);
        }
    }
}
