using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Seravian.Hubs;

[Authorize(Roles = "Patient")]
public class ChatHub : Hub<IChatHubClient>
{
    private static readonly ConcurrentDictionary<string, Guid> _connectionUserMap = new();
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

    [HubMethodName("send-client-request")]
    public async Task SendClientRequest(SendClientRequestDto request)
    {
        var receiveTimeUtc = DateTime.UtcNow;
        if (!_connectionUserMap.TryGetValue(Context.ConnectionId, out var chatId))
            return;

        if (
            !await _applicationDbContext.Chats.AnyAsync(c => c.Id == chatId && c.IsDeleted == false)
        )
        {
            // If the chat is deleted, remove the connection from the map
            _connectionUserMap.Remove(Context.ConnectionId, out _);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());

            // Find all keys that have the specified value
            var keysToRemove = _connectionUserMap
                .Where(kvp => kvp.Value == chatId)
                .Select(kvp => kvp.Key)
                .ToList(); // Use ToList to avoid modifying the collection during iteration
            foreach (var key in keysToRemove)
            {
                _connectionUserMap.Remove(key, out _); // Removes the entry from the dictionary
            }
            return;
        }

        var message = new ChatMessage
        {
            TimestampUtc = receiveTimeUtc,
            ChatId = chatId,
            Content = request.Message,
        };

        await _applicationDbContext.ChatsMessages.AddAsync(message);
        await _applicationDbContext.SaveChangesAsync();

        await Clients
            .Client(Context.ConnectionId)
            .ConfirmClientRequestAsync(
                new ConfirmClientRequestDto
                {
                    MessageId = message.Id,
                    ClientMessageId = request.MessageClientId,
                    TimestampUtc = receiveTimeUtc,
                }
            );

        await Clients
            .OthersInGroup(chatId.ToString())
            .ReceiveClientRequestAsync(
                new ReceiveClientRequestDto
                {
                    Id = message.Id,
                    Message = request.Message,
                    TimestampUtc = receiveTimeUtc,
                }
            );

        await Task.Delay(TimeSpan.FromSeconds(3)); // Simulate some delay for the AI response

        await Clients
            .Group(chatId.ToString())
            .ReceiveAIResponseAsync(
                new ReceiveAIResponseDto
                {
                    Id = Random.Shared.Next(1, 1000),
                    Message = "This is a test message from the ai response",
                    TimestampUtc = receiveTimeUtc,
                }
            );
    }
}

public interface IChatHubClient
{
    [HubMethodName("confirm-client-request")]
    Task ConfirmClientRequestAsync(ConfirmClientRequestDto request);

    [HubMethodName("receive-client-request")]
    Task ReceiveClientRequestAsync(ReceiveClientRequestDto request);

    [HubMethodName("receive-ai-response")]
    Task ReceiveAIResponseAsync(ReceiveAIResponseDto request);
}
