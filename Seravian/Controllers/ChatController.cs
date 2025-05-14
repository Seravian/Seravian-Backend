using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("[controller]")]
[ApiController]
[Authorize(Roles = "Patient")]
public class ChatController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public ChatController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
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

        var messages = chat
            .ChatMessages.Where(m =>
                m.TimestampUtc > request.LastMessageTimestampUtc && !m.IsDeleted
            )
            .OrderBy(m => m.TimestampUtc)
            .Select(m => new ChatMessageDto
            {
                Id = m.Id,
                Content = m.Content,
                TimestampUtc = m.TimestampUtc,
                IsAI = m.IsAI,
            })
            .ToList();

        return Ok(messages);
    }
}
