using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public static class UtcDateTimeConverter
{
    // For non-nullable DateTime
    public static readonly ValueConverter<DateTime, DateTime> DateTimeConverter =
        new ValueConverter<DateTime, DateTime>(
            toDb => toDb,
            fromDb => DateTime.SpecifyKind(fromDb, DateTimeKind.Utc)
        );

    // For nullable DateTime
    public static readonly ValueConverter<DateTime?, DateTime?> NullableDateTimeConverter =
        new ValueConverter<DateTime?, DateTime?>(
            toDb => toDb,
            fromDb => fromDb.HasValue ? DateTime.SpecifyKind(fromDb.Value, DateTimeKind.Utc) : null
        );
}
