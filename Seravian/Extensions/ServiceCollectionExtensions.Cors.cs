namespace Seravian.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCors(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>();

        services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowFrontend",
                policy =>
                {
                    if (allowedOrigins.Contains("*"))
                    {
                        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                    }
                    else
                    {
                        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
                    }
                }
            );
        });

        return services;
    }

    public static IApplicationBuilder UseCustomCors(this IApplicationBuilder app)
    {
        app.UseCors("AllowFrontend");
        return app;
    }
}
