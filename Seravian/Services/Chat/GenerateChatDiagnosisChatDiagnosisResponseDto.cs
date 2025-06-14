namespace Seravian.DTOs.Services.Dtos;

public class GenerateChatDiagnosisChatDiagnosisResponseDto
{
    public Guid ChatId { get; set; }

    public string? DiagnosisMessagePrompt { get; set; }
    public bool IsSucceeded { get; set; }

    public string? FailureReason { get; set; }
    public string? DiagnosedProblem { get; set; }
    public string? Reasoning { get; set; }
    public List<string>? Prescription { get; set; }
}
