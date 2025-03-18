public static class CorsExtensions
{
    public static IServiceCollection AddCustomCors(
        this IServiceCollection services,
        bool isDevelopment
    )
    {
        services.AddCors(options =>
        {
            options.AddPolicy(
                "CustomCorsPolicy",
                corsPolicyBuilder =>
                {
                    if (isDevelopment)
                    {
                        corsPolicyBuilder
                            .AllowAnyOrigin() // Allow all origins in development
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    }
                    else
                    {
                        corsPolicyBuilder
                            .WithOrigins("https://your-production-frontend.com") // Restrict in production
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    }
                }
            );
        });

        return services;
    }

    public static IApplicationBuilder UseCustomCors(this IApplicationBuilder app)
    {
        app.UseCors("CustomCorsPolicy");
        return app;
    }
}
