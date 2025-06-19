using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace TestAIModels;

public class TTSService
{
    private readonly string _apiUrl;
    private readonly string _apiKey;
    private readonly string _apiKeyHeader;

    public TTSService(IOptionsMonitor<TTSSettings> llmOptions)
    {
        _apiKeyHeader = llmOptions.CurrentValue.ApiKeyHeader;
        _apiUrl = llmOptions.CurrentValue.ApiUrl;
        _apiKey = llmOptions.CurrentValue.ApiKey;
    }

    public async Task<string> GenerateVoiceFromText(string text)
    {
        var ttsRequest = new TTSRequest
        {
            Text = text,
            Voice = "tom",
            Preset = "ultra_fast",
            NumAutoregressiveSamples = 50,
            Seed = null,
            Temperature = 0.8f,
            LengthPenalty = 1.0f,
            RepetitionPenalty = 2.0f,
            TopP = 0.8f,
            MaxMelTokens = 500,
        };

        try
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(20);
            // Set API key header
            client.DefaultRequestHeaders.Add(_apiKeyHeader, _apiKey);

            // Serialize request to JSON
            var jsonRequest = JsonSerializer.Serialize(
                ttsRequest,
                options: new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                }
            );

            using var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // POST request
            HttpResponseMessage response = await client.PostAsync(_apiUrl, content);

            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();

            // Deserialize response
            TTSResponse ttsResponse = JsonSerializer.Deserialize<TTSResponse>(
                jsonResponse,
                options: new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                }
            );

            Console.WriteLine($"Sample Rate: {ttsResponse.SampleRate}");
            Console.WriteLine($"Audio Base64 Length: {ttsResponse.AudioBase64.Length}");

            // Optionally save audio to file (WAV)


            return ttsResponse.AudioBase64;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling TTS API: {ex.Message}");
            return null;
        }
    }
}

public class TTSRequest
{
    public string Text { get; set; }
    public string Voice { get; set; } = "random";
    public string Preset { get; set; } = "fast";
    public int NumAutoregressiveSamples { get; set; } = 50;
    public int? Seed { get; set; }
    public float Temperature { get; set; } = 0.8f;
    public float LengthPenalty { get; set; } = 1.0f;
    public float RepetitionPenalty { get; set; } = 2.0f;
    public float TopP { get; set; } = 0.8f;
    public int MaxMelTokens { get; set; } = 500;
}

public class TTSResponse
{
    public string AudioBase64 { get; set; }
    public int SampleRate { get; set; }
}
