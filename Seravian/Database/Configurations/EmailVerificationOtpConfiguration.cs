using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EmailVerificationOtpConfiguration : IEntityTypeConfiguration<EmailVerificationOtp>
{
    public void Configure(EntityTypeBuilder<EmailVerificationOtp> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Code).HasMaxLength(16).IsRequired();
        builder.Property(o => o.IsConsumed).IsRequired();
        builder.Property(o => o.ExpiresAtUtc).IsRequired();
        builder
            .HasOne(o => o.User)
            .WithMany(u => u.EmailVerificationOtps)
            .HasForeignKey(o => o.UserId);
    }
}
