using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace TestAIModels;

public class AIAudioAnalyzingService
{
    private readonly string _apiUrl;
    private readonly string _apiKey;
    private readonly string _apiKeyHeader;

    public AIAudioAnalyzingService(IOptionsMonitor<SERAndSTTSettings> llmOptions)
    {
        _apiKeyHeader = llmOptions.CurrentValue.ApiKeyHeader;
        _apiUrl = llmOptions.CurrentValue.ApiUrl;
        _apiKey = llmOptions.CurrentValue.ApiKey;
    }

    public async Task<AudioAnalyzingResult> AnalyzeAudio(string filePath)
    {
        // Generate or specify a GUID to send with the request

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }

        Guid requestGuid = Guid.NewGuid(); // Or specify a GUID manually like Guid.Parse("your-guid-here")

        var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMinutes(10);
        using var form = new MultipartFormDataContent();
        using var fileStream = File.OpenRead(filePath);
        using var fileContent = new StreamContent(fileStream);

        httpClient.DefaultRequestHeaders.Add(_apiKeyHeader, _apiKey);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav"); // Change based on file type
        form.Add(fileContent, "file", Path.GetFileName(filePath));

        // Add the GUID to the form data
        form.Add(new StringContent(requestGuid.ToString()), "id"); // Adding the GUID field

        var before = DateTime.Now;

        //AudioModelsResponse
        var response = await httpClient.PostAsync(_apiUrl, form);

        var after = DateTime.Now;

        Console.WriteLine($"Time taken: {(after - before).TotalMilliseconds} ms");
        Console.WriteLine($"Response Status Code: {response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error: {response.StatusCode} - {response.ReasonPhrase}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AudioAnalyzingResult>(
            jsonResponse,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            }
        );

        if (result == null)
        {
            throw new Exception("Failed to deserialize response.");
        }

        Console.WriteLine("Transcription: " + result.Transcription);
        Console.WriteLine("Dominant Emotion: " + result.DominantEmotion);
        Console.WriteLine("Emotions");

        foreach (var emotion in result.Emotions)
        {
            Console.WriteLine($"- {emotion.Emotion}: {emotion.Score}");
        }

        return result;
    }
}

public class AudioAnalyzingResult
{
    public SEREmotion DominantEmotion { get; set; }

    public List<SEREmotionScore> Emotions { get; set; }

    public string Transcription { get; set; }
}

public class SEREmotionScore
{
    public SEREmotion Emotion { get; set; }

    public float Score { get; set; }
}

public enum SEREmotion
{
    Unknown = -1,
    Angry,
    Happy,
    Fearful,
    Neutral,
    Surprised,
    Disgusted,
    Calm,
    Sad,
}
