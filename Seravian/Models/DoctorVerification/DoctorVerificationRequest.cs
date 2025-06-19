public class DoctorVerificationRequest
{
    public long Id { get; set; }
    public Guid DoctorId { get; set; }
    public DoctorTitle Title { get; set; }
    public string Description { get; set; }
    public int SessionPrice { get; set; }
    public RequestStatus Status { get; set; }
    public DateTime RequestedAtUtc { get; set; }
    public DateTime? ReviewedAtUtc { get; set; } = null;
    public DateTime? DeletedAtUtc { get; set; } = null;
    public Guid? ReviewerId { get; set; }
    public string? RejectionNote { get; set; }
    public string? DoctorAttachmentsNote { get; set; }
    public virtual Doctor Doctor { get; set; }
    public virtual Admin? Reviewer { get; set; }
    public virtual ICollection<DoctorVerificationRequestAttachment> Attachments { get; set; } = [];

    public string TimeZone { get; set; } // IANA format
    public string Nationality { get; set; } // ISO 3166-1 alpha-2
    public virtual ICollection<DoctorLanguage> Languages { get; set; } = [];
    public virtual ICollection<WorkingHoursTimeSlot> WorkingHours { get; set; } = [];
    public byte[] RowVersion { get; set; } // concurrency token
}

public class DoctorLanguage
{
    public long Id { get; set; }
    public string LanguageCode { get; set; } = null!; // ISO 639-1
    public long DoctorVerificationRequestId { get; set; }

    public virtual DoctorVerificationRequest DoctorVerificationRequest { get; set; } = null!;
}

public class WorkingHoursTimeSlot
{
    public long Id { get; set; }

    public DayOfWeek DayOfWeek { get; set; } // enum
    public TimeOnly From { get; set; }
    public TimeOnly To { get; set; }

    public long DoctorVerificationRequestId { get; set; }
    public virtual DoctorVerificationRequest DoctorVerificationRequest { get; set; } = null!;
}

public static class DoctorVerificationRequestExtensions
{
    public static Dictionary<DayOfWeek, List<WorkingHoursTimeSlot>> GetWorkingHoursGroupedByDay(
        this DoctorVerificationRequest request
    )
    {
        // Group existing slots by DayOfWeek
        var grouped = request
            .WorkingHours.GroupBy(slot => slot.DayOfWeek)
            .ToDictionary(group => group.Key, group => group.OrderBy(slot => slot.From).ToList());

        // Ensure all days of the week are present
        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            if (!grouped.ContainsKey(day))
            {
                grouped[day] = new List<WorkingHoursTimeSlot>();
            }
        }

        return grouped;
    }
}
