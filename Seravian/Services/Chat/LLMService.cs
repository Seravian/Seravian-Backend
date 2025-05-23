using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace TestAIModels;

public class LLMService
{
    private readonly string _apiUrl;
    private readonly string _apiKey;
    private readonly string _apiKeyHeader;

    public LLMService(IOptionsMonitor<LLMSettings> llmOptions)
    {
        _apiKeyHeader = llmOptions.CurrentValue.ApiKeyHeader;
        _apiUrl = llmOptions.CurrentValue.ApiUrl;
        _apiKey = llmOptions.CurrentValue.ApiKey;
    }

    public async Task<string?> SendMessageToLLMAsync(string message, long messageId, string chatId)
    {
        var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMinutes(20);
        httpClient.DefaultRequestHeaders.Add(_apiKeyHeader, _apiKey);
        var payload = new LLMRequestDto
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

            var response = await httpClient.PostAsync(_apiUrl, content);
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
}

public class LLMRequestDto
{
    public string Message { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
    public long MessageId { get; set; }
}
