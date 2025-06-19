public class LLMSettings
{
    public string ApiKey { get; set; }
    public string ApiKeyHeader { get; set; }

    public string MentalLLaMA7BBaseUrl { get; set; }
    public string MentalLLaMA13BBaseUrl { get; set; }
    public string GenerateResponseEndpointName { get; set; }
    public string GenerateDiagnosisEndpointName { get; set; }
}
