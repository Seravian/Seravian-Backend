using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

public class DeepFaceService
{
    private readonly string _apiUrl;
    private readonly string _apiKey;
    private readonly string _apiKeyHeader;

    public DeepFaceService(IOptionsMonitor<DeepFaceSettings> llmOptions)
    {
        _apiKeyHeader = llmOptions.CurrentValue.ApiKeyHeader;
        _apiUrl = llmOptions.CurrentValue.ApiUrl;
        _apiKey = llmOptions.CurrentValue.ApiKey;
    }

    public async Task<DeepFaceResult> AnalyzeImages(params string[] imagePaths)
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMinutes(20);

        // Uncomment this if you're using API key authentication in headers
        httpClient.DefaultRequestHeaders.Add(_apiKeyHeader, _apiKey);

        using var form = new MultipartFormDataContent();

        foreach (var imagePath in imagePaths)
        {
            var fileStream = File.OpenRead(imagePath);
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg"); // Change if needed
            form.Add(streamContent, "files", Path.GetFileName(imagePath));
            form.Add(new StringContent(Guid.NewGuid().ToString()), "ids");
        }

        // Make the request
        Console.WriteLine("Sending request to deepface-analysis service...");
        var before = DateTime.Now;
        var response = await httpClient.PostAsync(_apiUrl, form);
        var after = DateTime.Now;

        Console.WriteLine($"Time taken: {(after - before).TotalMilliseconds} ms");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed with status: {response.StatusCode}");
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response body: {error}");
            throw new Exception($"Failed with status: {response.StatusCode}");
        }

        var responseString = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseString);

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<DeepFaceResult>(
            jsonResponse,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            }
        );

        Console.WriteLine("result:");
        Console.WriteLine("Restult.Results:");
        foreach (var resultItem in result.Results)
        {
            Console.WriteLine($"resultItem.Id: {resultItem.Id}");
            Console.WriteLine($"resultItem.Filename: {resultItem.Filename}");
            Console.WriteLine($"resultItem.Gender: {resultItem.Gender}");
            Console.WriteLine($"resultItem.Age: {resultItem.Age}");
            Console.WriteLine($"resultItem.DominantEmotion: {resultItem.DominantEmotion}");
            Console.WriteLine($"resultItem.Emotions:");
            foreach (var emotion in resultItem.Emotions)
            {
                Console.WriteLine($"emotion.Emotion: {emotion.Emotion}");
                Console.WriteLine($"emotion.Score: {emotion.Score}");
            }
        }
        return result;
    }
}

public class DeepFaceResult
{
    public List<ImageResult> Results { get; set; } = [];
}

public class ImageResult
{
    public Guid Id { get; set; } // public int MyProperty { get; set; }
    public string Filename { get; set; }
    public DeepFaceEmotion? Gender { get; set; }

    public float? Age { get; set; }
    public DeepFaceEmotion? DominantEmotion { get; set; }
    public List<DeepFaceEmotionScore>? Emotions { get; set; }
}

public enum DeepFaceEmotion
{
    Angry,
    Disgust,
    Fear,
    Happy,
    Sad,
    Surprise,
    Neutral,
}

public class DeepFaceEmotionScore
{
    public DeepFaceEmotion Emotion { get; set; }
    public float Score { get; set; }
}

public enum DeepFaceGender
{
    Man,
    Woman,
}
