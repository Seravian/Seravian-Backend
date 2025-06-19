namespace Seravian.DTOs.Chat;

public class GetChatResponseDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public DateTime CreatedAt { get; set; }
}
