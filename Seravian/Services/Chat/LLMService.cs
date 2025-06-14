using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Seravian.DTOs.Services.Dtos;

namespace TestAIModels;

public class LLMService
{
    private readonly string _apiGenerateDiagnosisUrl;
    private readonly string _apiGenerateResponseUrl;
    private readonly string _apiKey;
    private readonly string _apiKeyHeader;

    public LLMService(IOptionsMonitor<LLMSettings> llmOptions)
    {
        _apiKeyHeader = llmOptions.CurrentValue.ApiKeyHeader;
        _apiGenerateDiagnosisUrl = llmOptions.CurrentValue.GenerateDiagnosisUrl;
        _apiGenerateResponseUrl = llmOptions.CurrentValue.GenerateResponseUrl;
        _apiKey = llmOptions.CurrentValue.ApiKey;
    }

    public async Task<string?> GenerateChatMessageResponseAsync(
        string message,
        long messageId,
        string chatId
    )
    {
        var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMinutes(20);
        httpClient.DefaultRequestHeaders.Add(_apiKeyHeader, _apiKey);
        var payload = new GenerateChatMessageResponseRequestDto
        {
            Message = message,
            ChatId = chatId,
            MessageId = messageId,
        };

        try
        {
            var jsonPayload = JsonSerializer.Serialize(
                payload,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            );

            Console.WriteLine($"Sending message: {jsonPayload}");
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(_apiGenerateResponseUrl, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<Dictionary<string, string>>(
                responseContent
            );

            return responseJson is not null && responseJson.TryGetValue("response", out var reply)
                ? reply
                : string.Empty;
        }
        catch (HttpRequestException httpEx)
        {
            Console.WriteLine($"[HTTP Error] {httpEx.StatusCode}: {httpEx.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] {ex.Message}");
        }

        return string.Empty;
    }

    public async Task<GenerateChatDiagnosisChatDiagnosisResponseDto?> GenerateChatDiagnosisAsync(
        Guid chatId,
        long diagnosisId,
        List<GenerateChatDiagnosisMessageEntry> messages
    )
    {
        var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMinutes(20);
        httpClient.DefaultRequestHeaders.Add(_apiKeyHeader, _apiKey);
        var payload = new GenerateChatDiagnosisRequestDto
        {
            ChatId = chatId,
            ChatDiagnosisId = diagnosisId,
            Messages = messages,
        };

        var jsonPayload = JsonSerializer.Serialize(
            payload,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );
        Console.WriteLine($"Sending generate diagnosis : {jsonPayload}");
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(_apiGenerateDiagnosisUrl, content);

        var jsonResponse = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"API Error ({(int)response.StatusCode}): {jsonResponse}"
            );
        }

        GenerateChatDiagnosisChatDiagnosisResponseDto? diagnosis;
        try
        {
            diagnosis = JsonSerializer.Deserialize<GenerateChatDiagnosisChatDiagnosisResponseDto>(
                jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (diagnosis == null)
                throw new JsonException("Deserialized response is null.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to deserialize API response: {ex.Message}",
                ex
            );
        }

        // ---- VALIDATION SECTION ----
        if (diagnosis.IsSucceeded)
        {
            if (string.IsNullOrWhiteSpace(diagnosis.DiagnosedProblem))
                throw new InvalidOperationException(
                    "DiagnosedProblem is null or empty despite success."
                );
            if (string.IsNullOrWhiteSpace(diagnosis.Reasoning))
                throw new InvalidOperationException("Reasoning is null or empty despite success.");
            if (diagnosis.Prescription == null || diagnosis.Prescription.Count == 0)
                throw new InvalidOperationException(
                    "Prescription is null or empty despite success."
                );
            if (!string.IsNullOrWhiteSpace(diagnosis.FailureReason))
                throw new InvalidOperationException("FailureReason should be null on success.");
        }
        else
        {
            if (
                !string.IsNullOrWhiteSpace(diagnosis.DiagnosedProblem)
                || !string.IsNullOrWhiteSpace(diagnosis.Reasoning)
                || diagnosis.Prescription != null
            )
            {
                throw new InvalidOperationException(
                    "Diagnosis-related fields should be null on failure."
                );
            }
            if (string.IsNullOrWhiteSpace(diagnosis.FailureReason))
                throw new InvalidOperationException("FailureReason must be present on failure.");
        }

        return diagnosis;
    }
}
