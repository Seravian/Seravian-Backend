using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Seravian.DTOs.ChatHub;

namespace Seravian.Hubs;

[Authorize(Roles = "Patient")]
public class ChatHub : Hub<IChatHubClient>
{
    private readonly ApplicationDbContext _applicationDbContext;

    public ChatHub(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
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

        await Groups.AddToGroupAsync(connectionId, chatId.ToString());
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
