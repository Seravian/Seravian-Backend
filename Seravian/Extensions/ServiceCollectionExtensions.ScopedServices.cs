using TestAIModels;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddSingletonServices(this IServiceCollection services)
    {
        services.AddSingleton<ChatProcessingManager>();
        services.AddSingleton<LLMService>();
        services.AddSingleton<AIAudioAnalyzingService>();
        services.AddSingleton<DeepFaceService>();
        services.AddSingleton<TTSService>();

        return services;
    }

    public static IServiceCollection AddScopedServices(this IServiceCollection services)
    {
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAudioService, AudioService>();

        return services;
    }
}
