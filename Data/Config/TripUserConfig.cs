using FleetPulse_BackEndDevelopment.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TripUserConfig : IEntityTypeConfiguration<TripUser>
{
    public void Configure(EntityTypeBuilder<TripUser> builder)
    {
        builder.HasKey(tu => new { tu.UserId, tu.TripId });
        
        builder.HasOne(tu => tu.User)
            .WithMany(u => u.TripUsers)
            .HasForeignKey(tu => tu.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        
        builder.HasOne(tu => tu.Trip)
            .WithMany(t => t.TripUsers)
            .HasForeignKey(tu => tu.TripId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}