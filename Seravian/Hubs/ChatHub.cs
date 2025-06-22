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

    public override async Task OnConnectedAsync()
    {
        var patientId = Guid.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var httpContext = Context.GetHttpContext();
        var chatIdString = httpContext?.Request.Query["chatId"].ToString();

        if (
            !Guid.TryParse(chatIdString, out Guid chatId)
            || !await _applicationDbContext.Chats.AnyAsync(c =>
                c.Id == chatId && c.PatientId == patientId && c.IsDeleted == false
            )
        )
        {
            Context.Abort(); // ❌ Chat doesn't belong to user chatId is invalid Guid
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
        await base.OnConnectedAsync();
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
