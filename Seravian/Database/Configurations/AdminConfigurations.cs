using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

class AdminConfigurations : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        builder.HasKey(d => d.UserId); // UserId cannot be null;

        builder
            .HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<Admin>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
