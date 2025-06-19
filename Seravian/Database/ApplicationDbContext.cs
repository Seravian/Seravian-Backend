using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<EmailVerificationOtp> EmailVerificationOtpCodes { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<ChatMessageMedia> ChatsMessagesMedias { get; set; }
    public DbSet<ChatVoiceAnalysis> ChatVoiceAnalyses { get; set; }
    public DbSet<ChatMessage> ChatsMessages { get; set; }
    public DbSet<ChatDiagnosis> ChatDiagnoses { get; set; }

    public DbSet<DoctorVerificationRequest> DoctorsVerificationRequests { get; set; }
    public DbSet<DoctorVerificationRequestAttachment> DoctorsVerificationRequestsAttachments { get; set; }
    public DbSet<SessionBooking> SessionBookings { get; set; }
    public DbSet<DoctorLanguage> DoctorLanguages { get; set; }
    public DbSet<WorkingHoursTimeSlot> WorkingHoursTimeSlots { get; set; }
}
