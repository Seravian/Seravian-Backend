using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Seravian.DTOs.ChatHub;
using TestAIModels;

namespace Seravian.Hubs;

[Authorize(Roles = "Patient")]
public class ChatHub : Hub<IChatHubClient>
{
    private readonly IAIResponseTrackerService _aiResponseTracker;

    private static readonly ConcurrentDictionary<string, Guid> _connectionUserMap = new();
    private readonly ChatProcessingManager _llmProcessingManager;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    private readonly LLMService _llmService;

    private readonly ApplicationDbContext _applicationDbContext;

    public ChatHub(
        ChatProcessingManager llmProcessingManager,
        LLMService llmService,
        IAIResponseTrackerService aiResponseTracker,
        ApplicationDbContext applicationDbContext,
        IDbContextFactory<ApplicationDbContext> dbContextFactory
    )
    {
        _llmProcessingManager = llmProcessingManager;
        _llmService = llmService;
        _aiResponseTracker = aiResponseTracker;
        _applicationDbContext = applicationDbContext;
        _dbContextFactory = dbContextFactory;
    }

    [HubMethodName("join-chat")]
    public async Task JoinChat(JoinChatDto request)
    {
        var patientId = Guid.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var connectionId = Context.ConnectionId;
        var chatId = request.ChatId;
        if (
            !await _applicationDbContext.Chats.AnyAsync(c =>
                c.Id == chatId && c.PatientId == patientId && c.IsDeleted == false
            )
        )
            return;

        _connectionUserMap[connectionId] = chatId;
        await Groups.AddToGroupAsync(connectionId, chatId.ToString());
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var isConnectionExistsInMap = _connectionUserMap.TryGetValue(
            Context.ConnectionId,
            out var chatId
        );
        if (!isConnectionExistsInMap)
            return;
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
        _connectionUserMap.Remove(Context.ConnectionId, out _);
    }
}

public interface IChatHubClient
{
    [HubMethodName("receive-client-request")]
    Task ReceiveClientRequestAsync(ReceiveClientRequestDto request);

    [HubMethodName("receive-ai-response")]
    Task ReceiveAIResponseAsync(ReceiveAIResponseDto request);

    [HubMethodName("notify-ai-audio-response-ready")]
    Task NotifyAiAudioResponseReadyAsync(NotifyAiAudioResponseReadyDto request);

    [HubMethodName("notify-chat-diagnosis-ready")]
    Task NotifyChatDiagnosisReadyAsync(NotifyChatDiagnosisReadyDto request);
}
