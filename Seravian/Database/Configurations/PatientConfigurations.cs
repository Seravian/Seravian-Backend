using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

class PatientConfigurations : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(d => d.UserId); // UserId cannot be null;

        builder
            .HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<Patient>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
