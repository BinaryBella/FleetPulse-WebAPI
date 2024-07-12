using FleetPulse_BackEndDevelopment.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FCMNotificationConfig : IEntityTypeConfiguration<FCMNotification>
{
    public void Configure(EntityTypeBuilder<FCMNotification> builder)
    {
        builder.HasKey(n => n.NotificationId);

        builder.Property(n => n.Title).IsRequired().HasMaxLength(255); 
        builder.Property(n => n.Message).IsRequired().HasMaxLength(1000);
        builder.Property(n => n.Date).IsRequired();
        builder.Property(n => n.Time).IsRequired();
        builder.Property(n => n.Status).IsRequired();

        builder.ToTable("Notifications"); // Ensure table name matches your database schema
    }
}