using Microsoft.EntityFrameworkCore;

public class ExpiredSessionBookingRejectionService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredSessionBookingRejectionService> _logger;

    public ExpiredSessionBookingRejectionService(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpiredSessionBookingRejectionService> logger
    )
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RejectExpiredPendingBookingsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while rejecting expired pending bookings.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task RejectExpiredPendingBookingsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var utcNow = DateTime.UtcNow;
        try
        {
            var expiredBookings = await dbContext
                .SessionBookings.Where(x =>
                    x.Status == SessionBookingStatus.Pending
                    && x.PatientIsAvailableToUtc < utcNow.AddHours(1)
                )
                .ToListAsync();

            if (!expiredBookings.Any())
                return;

            foreach (var booking in expiredBookings)
            {
                booking.Status = SessionBookingStatus.Rejected;
                booking.UpdatedAtUtc = utcNow;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Rejected {Count} expired pending session bookings.",
                expiredBookings.Count
            );
        }
        //handle concurrency
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning(
                "Concurrency error occurred while rejecting expired pending session bookings."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while rejecting expired pending session bookings."
            );
        }
    }
}
