namespace Seravian.DTOs.Chat;

public class CreateChatResponseDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
