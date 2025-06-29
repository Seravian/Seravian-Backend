using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Seravian.DTOs.Chat;
using Seravian.DTOs.ChatHub;
using Seravian.DTOs.Services.Dtos;
using Seravian.Hubs;
using TestAIModels;

[Route("[controller]")]
[ApiController]
[Authorize(Roles = "Patient")]
public partial class ChatController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly ChatProcessingManager _llmProcessingManager;
    private readonly ChatDiagnosesLocksManager _llmDiagnosesLocksManager;
    private readonly IHubContext<ChatHub, IChatHubClient> _hubContext;
    private readonly AIAudioAnalyzingService _voiceAnalysisService;
    private readonly LLMService _llmService;
    private readonly TTSService _ttsService;
    private readonly IAudioService _audioService;
    private readonly IOptionsMonitor<AudioPathsSettings> _audioPathsSettings;
    private readonly ILogger<ChatController> _logger;
    private readonly IAIResponseTrackerService _aiResponseTracker;
    private readonly IAIDiagnosisTrackerService _aiDiagnosisTracker;

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
        IAIResponseTrackerService aiResponseTracker,
        ILogger<ChatController> logger,
        ChatDiagnosesLocksManager llmDiagnosesLocksManager,
        IAIDiagnosisTrackerService aiDiagnosisTracker
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
        _aiResponseTracker = aiResponseTracker;
        _logger = logger;
        _llmDiagnosesLocksManager = llmDiagnosesLocksManager;
        _aiDiagnosisTracker = aiDiagnosisTracker;
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
    public async Task<IActionResult> DeleteChatAsync([FromQuery] DeleteChatRequestDto request)
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

    [HttpPost("send-client-request")]
    public async Task<ActionResult<SendClientRequestResponseDto>> SendClientRequest(
        [FromBody] SendClientRequestRequestDto request
    )
    {
        var utcNow = DateTime.UtcNow;
        var patientId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        if (request.ChatId == null || request.ChatId == Guid.Empty)
        {
            return BadRequest(new { Errors = new List<string> { "Chat ID is required." } });
        }
        if (string.IsNullOrEmpty(request.Message))
        {
            return BadRequest(new { Errors = new List<string> { "Message is required." } });
        }

        if (
            !await _dbContext.Chats.AnyAsync(c =>
                c.Id == request.ChatId && c.PatientId == patientId && !c.IsDeleted
            )
        )
        {
            return BadRequest(new { Errors = new List<string> { "Chat not found." } });
        }

        if (!await _llmProcessingManager.TryLock(request.ChatId))
        {
            return Conflict("AI response is still being generated for this chat. Please wait.");
        }

        _aiResponseTracker.TryStartResponse(request.ChatId);
        var message = new ChatMessage
        {
            TimestampUtc = utcNow,
            ChatId = request.ChatId,
            Content = request.Message,
            MessageType = MessageType.Text,
        };

        await _dbContext.ChatsMessages.AddAsync(message);
        await _dbContext.SaveChangesAsync();

        await _hubContext
            .Clients.Groups(request.ChatId.ToString())
            .ReceiveClientRequestAsync(
                new ReceiveClientRequestDto
                {
                    Id = message.Id,
                    Message = request.Message,
                    TimestampUtc = utcNow,
                    MessageType = MessageType.Text,
                    ChatId = request.ChatId,
                }
            );

        _ = Task.Run(async () =>
        {
            try
            {
                var response = await _llmService.GenerateChatMessageResponseAsync(
                    request.Message,
                    message.Id,
                    request.ChatId.ToString()
                );
                var aiResponseReceivedTimeUtc = DateTime.UtcNow;

                var aiResponse = new ChatMessage
                {
                    TimestampUtc = aiResponseReceivedTimeUtc,
                    ChatId = request.ChatId,
                    Content = response,
                    IsAI = true,
                    MessageType = MessageType.Text,
                };

                using var dbContext = _dbContextFactory.CreateDbContext();

                await dbContext.ChatsMessages.AddAsync(aiResponse);
                await dbContext.SaveChangesAsync();

                await _hubContext
                    .Clients.Group(request.ChatId.ToString())
                    .ReceiveAIResponseAsync(
                        new ReceiveAIResponseDto
                        {
                            Id = aiResponse.Id,
                            Message = response,
                            TimestampUtc = aiResponseReceivedTimeUtc,
                            MessageType = MessageType.Text,
                            ChatId = request.ChatId,
                        }
                    );
            }
            catch (Exception ex)
            {
                Console.WriteLine("AI Task Error: " + ex); // Replace with real logger
            }
            finally
            {
                _llmProcessingManager.Release(request.ChatId);
                _aiResponseTracker.MarkResponseComplete(request.ChatId);
            }
        });

        return Ok(
            new SendClientRequestResponseDto
            {
                ChatId = request.ChatId,
                ClientMessageId = request.ClientMessageId,
                MessageId = message.Id,
                TimestampUtc = utcNow,
            }
        );
    }

    [HttpGet("is-processing")]
    public async Task<ActionResult<IsProcessingResponseDto>> IsProcessing(
        [FromQuery] IsProcessingRequestDto request
    )
    {
        var patientId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var chatId = request.ChatId;

        if (
            !await _dbContext.Chats.AnyAsync(c =>
                c.Id == chatId && c.PatientId == patientId && !c.IsDeleted
            )
        )
        {
            return BadRequest(new { Errors = new List<string> { "Chat not found." } });
        }

        var isProcessing = _aiResponseTracker.IsResponding(chatId);

        return Ok(new IsProcessingResponseDto { IsProcessing = isProcessing });
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

        _aiResponseTracker.TryStartResponse(chatId);
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

                        var formatLLMInput = $"{analysisResult.Transcription}";
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
                                LLMFormattedInputFromCombinedAnalysisResult = formatLLMInput,
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

                        var llmResponse = await _llmService.GenerateChatMessageResponseAsync(
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
                        _aiResponseTracker.MarkResponseComplete(chatId);
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
                _aiResponseTracker.MarkResponseComplete(chatId);

                return BadRequest("Failed to process voice file.");
            }
        }
        catch (Exception ex)
        {
            _llmProcessingManager.Release(chatId);
            _aiResponseTracker.MarkResponseComplete(chatId);
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("get-chat-diagnosis-details")]
    public async Task<
        ActionResult<GetChatDiagnosisDetailsResponseDto>
    > GetChatDiagnosisDetailsAsync([FromQuery] GetChatDiagnosisDetailsRequestDto request)
    {
        try
        {
            var patientId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var chatDiagnosis = await _dbContext
                .ChatDiagnoses.AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.Chat)
                .Include(c => c.Prescriptions)
                .FirstOrDefaultAsync(c =>
                    c.Id == request.ChatDiagnosisId
                    && c.Chat.PatientId == patientId
                    && !c.IsDeleted
                    && !c.Chat.IsDeleted
                );

            if (chatDiagnosis is null)
            {
                return BadRequest(new { Errors = new List<string> { "Chat not found." } });
            }

            return Ok(
                new GetChatDiagnosisDetailsResponseDto
                {
                    Id = chatDiagnosis.Id,
                    DiagnosedProblem = chatDiagnosis.DiagnosedProblem,
                    Reasoning = chatDiagnosis.Reasoning,
                    Prescriptions = chatDiagnosis
                        .Prescriptions.OrderBy(p => p.OrderIndex)
                        .Select(p => p.Content)
                        .ToList(),
                    FailureReason = chatDiagnosis.FailureReason,
                    RequestedAtUtc = chatDiagnosis.RequestedAtUtc,
                    CompletedAtUtc = chatDiagnosis.CompletedAtUtc,
                }
            );
        }
        catch
        {
            return BadRequest(new { Errors = new List<string> { "an error occurred" } });
        }
    }

    [HttpGet("get-chat-diagnoses")]
    public async Task<ActionResult<List<GetChatDiagnosisResponseDto>>> GetChatDiagnosesAsync(
        [FromQuery] GetChatDiagnosisRequestDto request
    )
    {
        try
        {
            var patientId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var chatDiagnoses = await _dbContext
                .ChatDiagnoses.Include(c => c.Chat)
                .AsNoTracking()
                .Where(c =>
                    c.Chat.PatientId == patientId
                    && request.ChatId == c.ChatId
                    && !c.IsDeleted
                    && !c.Chat.IsDeleted
                )
                .Select(c => new GetChatDiagnosisResponseDto
                {
                    Id = c.Id,
                    DiagnosedProblem = c.DiagnosedProblem,
                    FailureReason = c.FailureReason,
                    RequestedAtUtc = c.RequestedAtUtc,
                    CompletedAtUtc = c.CompletedAtUtc,
                })
                .OrderByDescending(c => c.RequestedAtUtc)
                .ToListAsync();

            return Ok(chatDiagnoses);
        }
        catch
        {
            return BadRequest(new { Errors = new List<string> { "an error occurred" } });
        }
    }

    [HttpGet("is-diagnosing")]
    public async Task<ActionResult<IsDiagnosingResponseDto>> IsDiagnosingAsync(
        [FromQuery] IsDiagnosingRequestDto request
    )
    {
        var patientId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var chatId = request.ChatId;

        if (
            !await _dbContext.Chats.AnyAsync(c =>
                c.Id == chatId && c.PatientId == patientId && !c.IsDeleted
            )
        )
        {
            return BadRequest(new { Errors = new List<string> { "Chat not found." } });
        }

        var isDiagnosing = _aiDiagnosisTracker.IsDiagnosing(chatId);

        return Ok(new IsDiagnosingResponseDto { IsDiagnosing = isDiagnosing });
    }

    [HttpPost("create-chat-diagnosis")]
    public async Task<ActionResult<CreateChatDiagnosisResponseDto>> CreateChatDiagnosisAsync(
        [FromBody] CreateChatDiagnosisRequestDto request
    )
    {
        var patientId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var utcNow = DateTime.UtcNow;
        var chat = await _dbContext
            .Chats.AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.ChatMessages)
            .Include(c => c.ChatDiagnoses)
            .FirstOrDefaultAsync(c =>
                c.Id == request.ChatId && c.PatientId == patientId && !c.IsDeleted
            );
        if (chat is null)
        {
            return BadRequest(new { Errors = new List<string> { "Chat not found." } });
        }

        if (!chat.ChatMessages.Where(m => !m.IsDeleted && !m.IsAI).Any())
        {
            return BadRequest(new { Errors = new List<string> { "No patient messages found." } });
        }

        if (chat.ChatDiagnoses.Any(c => c.CompletedAtUtc == null))
        {
            return BadRequest(
                new
                {
                    Errors = new List<string>
                    {
                        "AI diagnosis is still being generated for this chat.",
                    },
                }
            );
        }
        if (!await _llmDiagnosesLocksManager.TryLock(request.ChatId))
        {
            return Conflict("AI diagnosis is still being generated for this chat. Please wait.");
        }

        _aiDiagnosisTracker.TryStartDiagnosing(request.ChatId);
        try
        {
            var chatDiagnosis = new ChatDiagnosis
            {
                ChatId = request.ChatId,
                RequestedAtUtc = utcNow,
                CompletedAtUtc = null,
                StartMessageId = chat
                    .ChatMessages.Where(m => !m.IsDeleted && !m.IsAI)
                    .Min(m => m.Id),
                ToMessageId = chat.ChatMessages.Where(m => !m.IsDeleted && !m.IsAI).Max(m => m.Id),
            };

            await _dbContext.ChatDiagnoses.AddAsync(chatDiagnosis);
            await _dbContext.SaveChangesAsync();

            try
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // get the diagnosis from the llm service
                        using var dbContext = _dbContextFactory.CreateDbContext();
                        var messagesEntries = await dbContext
                            .ChatsMessages.Where(m =>
                                !m.IsDeleted && m.ChatId == chatDiagnosis.ChatId
                            )
                            .OrderBy(m => m.Id)
                            .Select(m => new GenerateChatDiagnosisMessageEntry
                            {
                                Content = m.Content ?? "",
                                IsAi = m.IsAI,
                            })
                            .ToListAsync();

                        var response = await _llmService.GenerateChatDiagnosisAsync(
                            chatDiagnosis.ChatId,
                            chatDiagnosis.Id,
                            messagesEntries
                        );

                        if (response is null)
                        {
                            throw new Exception("Response is null.");
                        }

                        var diagnosis = await dbContext.ChatDiagnoses.FirstOrDefaultAsync(c =>
                            c.Id == chatDiagnosis.Id
                        );

                        if (diagnosis is null)
                        {
                            throw new Exception("Diagnosis not found.");
                        }

                        #region  set  diagnosis data

                        if (response.IsSucceeded)
                        {
                            diagnosis.DiagnosedProblem = response.DiagnosedProblem;
                            diagnosis.Reasoning = response.Reasoning;
                            diagnosis.FailureReason = null;

                            if (response.Prescription is null || !response.Prescription.Any())
                            {
                                throw new Exception("Prescription is null or empty.");
                            }
                            diagnosis.Prescriptions = response
                                .Prescription.Select(
                                    (p, i) =>
                                        new ChatDiagnosisPrescription
                                        {
                                            Content = p,
                                            OrderIndex = i + 1,
                                        }
                                )
                                .ToList();
                        }
                        else
                        {
                            if (response.FailureReason is null)
                            {
                                throw new Exception("Failure reason is null.");
                            }

                            diagnosis.DiagnosedProblem = null;
                            diagnosis.Reasoning = null;
                            diagnosis.FailureReason = response.FailureReason;
                        }

                        diagnosis.CompletedAtUtc = DateTime.UtcNow;

                        await dbContext.SaveChangesAsync();

                        #endregion
                        await _hubContext
                            .Clients.Group(diagnosis.ChatId.ToString())
                            .NotifyChatDiagnosisReadyAsync(
                                new NotifyChatDiagnosisReadyDto
                                {
                                    Id = diagnosis.Id,
                                    DiagnosedProblem = diagnosis.DiagnosedProblem,
                                    Reasoning = diagnosis.Reasoning,
                                    Prescriptions = diagnosis
                                        .Prescriptions.OrderBy(p => p.OrderIndex)
                                        .Select(p => p.Content)
                                        .ToList(),
                                    FailureReason = diagnosis.FailureReason,
                                    RequestedAtUtc = diagnosis.RequestedAtUtc,
                                    CompletedAtUtc = diagnosis.CompletedAtUtc!.Value,
                                }
                            );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error generating chat diagnosis.");
                    }
                    finally
                    {
                        _llmDiagnosesLocksManager.Release(request.ChatId);
                        _aiDiagnosisTracker.MarkDiagnosisComplete(request.ChatId);
                    }
                });

                return Ok(
                    new CreateChatDiagnosisResponseDto { ChatDiagnosisId = chatDiagnosis.Id }
                );
            }
            catch (Exception ex)
            {
                _llmDiagnosesLocksManager.Release(request.ChatId);
                _aiDiagnosisTracker.MarkDiagnosisComplete(request.ChatId);
                return BadRequest("Error generating chat diagnosis.");
            }
        }
        catch
        {
            _llmDiagnosesLocksManager.Release(request.ChatId);
            _aiDiagnosisTracker.MarkDiagnosisComplete(request.ChatId);

            return BadRequest();
        }
    }

    [HttpDelete("delete-completed-chat-diagnosis")]
    public async Task<IActionResult> DeleteCompletedChatDiagnosisAsync(
        [FromQuery] DeleteCompletedChatDiagnosisRequestDto request
    )
    {
        var patientId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var diagnosis = await _dbContext
            .ChatDiagnoses.Include(x => x.Chat)
            .FirstOrDefaultAsync(x =>
                x.Id == request.ChatDiagnosisId
                && x.Chat.PatientId == patientId
                && x.CompletedAtUtc != null
                && !x.IsDeleted
            );

        if (diagnosis is null)
        {
            return BadRequest(new { Errors = new List<string> { "Diagnosis not found." } });
        }

        diagnosis.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("delete-completed-chat-diagnoses")]
    public async Task<IActionResult> DeleteCompletedChatDiagnosesAsync(
        [FromQuery] DeleteCompletedChatDiagnosesRequestDto request
    )
    {
        var patientId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var chat = await _dbContext
            .Chats.Include(x => x.ChatDiagnoses)
            .FirstOrDefaultAsync(x =>
                x.PatientId == patientId && x.Id == request.ChatId && !x.IsDeleted
            );

        if (chat is null)
        {
            return BadRequest(new { Errors = new List<string> { "Chat not found." } });
        }
        // mark all completed and not deleted diagnoses as deleted
        foreach (
            var diagnosis in chat.ChatDiagnoses.Where(x => x.CompletedAtUtc != null && !x.IsDeleted)
        )
        {
            diagnosis.IsDeleted = true;
        }

        await _dbContext.SaveChangesAsync();
        return Ok();
    }
}
