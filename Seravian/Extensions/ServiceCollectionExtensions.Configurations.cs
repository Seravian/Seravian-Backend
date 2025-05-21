using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Seravian.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfigurations(
        this IServiceCollection services,
        IConfiguration configurations
    )
    {
        services.Configure<EmailSettings>(configurations.GetSection("Email"));
        services.Configure<OtpSettings>(configurations.GetSection("Otp"));
        services.Configure<JwtSettings>(configurations.GetSection("Jwt"));
        services.Configure<LLMSettings>(configurations.GetSection("AI:LLM"));
        services.Configure<SERAndSTTSettings>(configurations.GetSection("AI:SERAndSTT"));
        services.Configure<DeepFaceSettings>(configurations.GetSection("AI:DeepFace"));
        services.Configure<TTSSettings>(configurations.GetSection("AI:TTS"));
        services.Configure<AudioPathsSettings>(configurations.GetSection("AudioPaths"));
        services.Configure<AIAudioResponsesCleanupSettings>(
            configurations.GetSection("AIAudioResponsesCleanup")
        );
        return services;
    }
}
