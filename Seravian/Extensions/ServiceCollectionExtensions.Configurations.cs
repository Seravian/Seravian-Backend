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
        return services;
    }
}
