namespace Seravian.DTOs.Services.Dtos;

public class GenerateChatMessageResponseRequestDto
{
    public string Message { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
    public long MessageId { get; set; }
}
