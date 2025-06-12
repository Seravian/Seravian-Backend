using TestAIModels;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddSingletonServices(this IServiceCollection services)
    {
        services.AddSingleton<ChatProcessingManager>();
        services.AddSingleton<ChatDiagnosesLocksManager>();
        services.AddSingleton<LLMService>();
        services.AddSingleton<AIAudioAnalyzingService>();
        services.AddSingleton<DeepFaceService>();
        services.AddSingleton<TTSService>();
        services.AddSingleton<IAIResponseTrackerService, AIResponseTrackerService>();
        services.AddSingleton<IAIDiagnosisTrackerService, AIDiagnosisTrackerService>();
        services.AddSingleton<DoctorsVerificationRequestsAttachmentFilesAccessLockingManager>();

        return services;
    }

    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<AIAudioResponsesCleanupService>();
        services.AddHostedService<CleanDbBackgroundService>();
        services.AddHostedService<ExpiredSessionBookingRejectionService>();

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
