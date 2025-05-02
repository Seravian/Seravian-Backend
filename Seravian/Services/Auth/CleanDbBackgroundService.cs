using Microsoft.EntityFrameworkCore;

public class CleanDbBackgroundService : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromHours(12);
    private IServiceScopeFactory _scopeFactory;

    public CleanDbBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var currentTimeUtc = DateTime.UtcNow;
            var dayBeforeThreshold = currentTimeUtc.AddDays(-1);
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await context
                .Users.Where(x => x.CreatedAtUtc < dayBeforeThreshold && !x.IsEmailVerified)
                .ExecuteDeleteAsync();

            await context
                .EmailVerificationOtpCodes.Where(x => x.ExpiresAtUtc < currentTimeUtc)
                .ExecuteDeleteAsync();

            await context
                .RefreshTokens.Where(x => x.ExpiresAtUtc < currentTimeUtc)
                .ExecuteDeleteAsync();

            await context.SaveChangesAsync();
            await Task.Delay(_interval, stoppingToken);
        }
    }
}
