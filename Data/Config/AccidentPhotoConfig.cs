using FleetPulse_BackEndDevelopment.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetPulse_BackEndDevelopment.Data.Config;

public class AccidentPhotoConfig : IEntityTypeConfiguration<AccidentPhoto>
{
    public void Configure(EntityTypeBuilder<AccidentPhoto> builder)
    {
        builder.ToTable("AccidentPhotos");
        builder.HasKey(a => a.Id);

        builder.HasOne(ap => ap.Accident)
            .WithMany(a => a.AccidentPhotos)
            .HasForeignKey(ap => ap.AccidentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}