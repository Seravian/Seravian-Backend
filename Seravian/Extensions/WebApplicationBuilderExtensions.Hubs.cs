using Seravian.Hubs;

public static class WebApplicationBuilderExtensions
{
    public static WebApplication MapSignalRHubs(this WebApplication app)
    {
        app.MapHub<ChatHub>(
            "hubs/chat",
            options =>
            {
                options.CloseOnAuthenticationExpiration = true;
            }
        );

        return app;
    }
}
