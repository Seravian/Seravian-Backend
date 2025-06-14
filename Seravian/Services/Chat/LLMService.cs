using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Seravian.DTOs.Services.Dtos;

namespace TestAIModels;

public class LLMService
{
    private readonly string _generateResponseEndpointName;
    private readonly string _generateDiagnosisEndpointName;
    private readonly string _mentalLLaMA7BBaseUrl;
    private readonly string _mentalLLaMA13BBaseUrl;
    private readonly string _apiKey;
    private readonly string _apiKeyHeader;

    public LLMService(IOptionsMonitor<LLMSettings> llmOptions)
    {
        _apiKeyHeader = llmOptions.CurrentValue.ApiKeyHeader;
        _mentalLLaMA7BBaseUrl = llmOptions.CurrentValue.MentalLLaMA7BBaseUrl;
        _mentalLLaMA13BBaseUrl = llmOptions.CurrentValue.MentalLLaMA13BBaseUrl;
        _generateResponseEndpointName = llmOptions.CurrentValue.GenerateResponseEndpointName;
        _generateDiagnosisEndpointName = llmOptions.CurrentValue.GenerateDiagnosisEndpointName;
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

            var response = await httpClient.PostAsync(
                _mentalLLaMA13BBaseUrl + _generateResponseEndpointName,
                content
            );
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
        var fallbackResponse = new GenerateChatDiagnosisChatDiagnosisResponseDto
        {
            ChatId = chatId,
            DiagnosisMessagePrompt = null,
            DiagnosedProblem = null,
            Reasoning = null,
            Prescription = [],
            IsSucceeded = false,
            FailureReason = "AI Diagnosis failed to generate due to unknown error.",
        };
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

        var response = await httpClient.PostAsync(
            _mentalLLaMA13BBaseUrl + _generateDiagnosisEndpointName,
            content
        );

        var jsonResponse = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            // throw new HttpRequestException(
            //     $"API Error ({(int)response.StatusCode}): {jsonResponse}"
            // );
            return fallbackResponse;
        }

        GenerateChatDiagnosisChatDiagnosisResponseDto? diagnosis;
        try
        {
            diagnosis = JsonSerializer.Deserialize<GenerateChatDiagnosisChatDiagnosisResponseDto>(
                jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (diagnosis == null)
            {
                // throw new JsonException("Deserialized response is null.");
                return fallbackResponse;
            }
        }
        catch (JsonException ex)
        {
            // throw new InvalidOperationException(
            //     $"Failed to deserialize API response: {ex.Message}",
            //     ex
            // );
            return fallbackResponse;
        }

        // ---- VALIDATION SECTION ----
        if (diagnosis.IsSucceeded)
        {
            if (string.IsNullOrWhiteSpace(diagnosis.DiagnosedProblem))
            {
                // throw new InvalidOperationException(
                //     "DiagnosedProblem is null or empty despite success."
                // );
                return fallbackResponse;
            }
            if (string.IsNullOrWhiteSpace(diagnosis.Reasoning))
            {
                // throw new InvalidOperationException("Reasoning is null or empty despite success.");
                return fallbackResponse;
            }
            if (diagnosis.Prescription == null || diagnosis.Prescription.Count == 0)
            {
                // throw new InvalidOperationException(
                //     "Prescription is null or empty despite success."
                // );
                return fallbackResponse;
            }
            if (!string.IsNullOrWhiteSpace(diagnosis.FailureReason))
            {
                // throw new InvalidOperationException("FailureReason should be null on success.");
                return fallbackResponse;
            }
        }
        else
        {
            if (
                !string.IsNullOrWhiteSpace(diagnosis.DiagnosedProblem)
                || !string.IsNullOrWhiteSpace(diagnosis.Reasoning)
                || diagnosis.Prescription != null
            )
            {
                // throw new InvalidOperationException(
                //     "Diagnosis-related fields should be null on failure."
                // );
                return fallbackResponse;
            }
            if (string.IsNullOrWhiteSpace(diagnosis.FailureReason))
            {
                // throw new InvalidOperationException("FailureReason must be present on failure.");
                return fallbackResponse;
            }
        }

        return diagnosis;
    }
}
