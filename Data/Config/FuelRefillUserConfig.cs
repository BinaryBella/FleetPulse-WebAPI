using FleetPulse_BackEndDevelopment.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FuelRefillUserConfig : IEntityTypeConfiguration<FuelRefillUser>
{
    public void Configure(EntityTypeBuilder<FuelRefillUser> builder)
    {
        // Define the composite primary key
        builder.HasKey(fru => new { fru.UserId, fru.FuelRefillId });

        // Configure the relationship between FuelRefillUser and User
        builder.HasOne(fru => fru.User)
            .WithMany(u => u.FuelRefillUsers)
            .HasForeignKey(fru => fru.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure the relationship between FuelRefillUser and FuelRefill
        builder.HasOne(fru => fru.FuelRefill)
            .WithMany(fr => fr.FuelRefillUsers)
            .HasForeignKey(fru => fru.FuelRefillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}