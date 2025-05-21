using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TestAIModels;

namespace Seravian.Hubs;

[Authorize(Roles = "Patient")]
public class ChatHub : Hub<IChatHubClient>
{
    private static readonly ConcurrentDictionary<string, Guid> _connectionUserMap = new();
    private readonly ChatProcessingManager _llmProcessingManager;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    private readonly LLMService _llmService;

    private readonly ApplicationDbContext _applicationDbContext;

    public ChatHub(
        ChatProcessingManager llmProcessingManager,
        LLMService llmService,
        ApplicationDbContext applicationDbContext,
        IDbContextFactory<ApplicationDbContext> dbContextFactory
    )
    {
        _llmProcessingManager = llmProcessingManager;
        _llmService = llmService;
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

    [HubMethodName("send-client-request")]
    public async Task<bool> SendClientRequest(SendClientRequestDto request)
    {
        var receiveTimeUtc = DateTime.UtcNow;
        if (string.IsNullOrEmpty(request.Message))
            return true;

        if (!_connectionUserMap.TryGetValue(Context.ConnectionId, out var chatId))
            return true;

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
            return true;
        }

        if (!await _llmProcessingManager.TryLock(chatId))
        {
            return false;
        }

        var message = new ChatMessage
        {
            TimestampUtc = receiveTimeUtc,
            ChatId = chatId,
            Content = request.Message,
            MessageType = MessageType.Text,
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
                    MessageType = MessageType.Text,
                }
            );

        _ = Task.Run(async () =>
        {
            try
            {
                var response = await _llmService.SendMessageToLLMAsync(
                    request.Message,
                    chatId.ToString()
                );
                var aiResponseReceivedTimeUtc = DateTime.UtcNow;

                var aiResponse = new ChatMessage
                {
                    TimestampUtc = aiResponseReceivedTimeUtc,
                    ChatId = chatId,
                    Content = response,
                    IsAI = true,
                    MessageType = MessageType.Text,
                };

                using var dbContext = _dbContextFactory.CreateDbContext();

                await dbContext.ChatsMessages.AddAsync(aiResponse);
                await dbContext.SaveChangesAsync();

                await Clients
                    .Group(chatId.ToString())
                    .ReceiveAIResponseAsync(
                        new ReceiveAIResponseDto
                        {
                            Id = aiResponse.Id,
                            Message = response,
                            TimestampUtc = aiResponseReceivedTimeUtc,
                            MessageType = MessageType.Text,
                        }
                    );
            }
            catch (Exception ex)
            {
                Console.WriteLine("AI Task Error: " + ex); // Replace with real logger
            }
            finally
            {
                _llmProcessingManager.Release(chatId);
            }
        });
        return true;
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

    [HubMethodName("notify-ai-audio-response-ready")]
    Task NotifyAiAudioResponseReadyAsync(NotifyAiAudioResponseReadyDto request);
}

public class NotifyAiAudioResponseReadyDto
{
    public long AIAudioId { get; set; }
    public Guid ChatId { get; set; }
}
