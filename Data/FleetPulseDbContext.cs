using FleetPulse_BackEndDevelopment.Data.Config;
using FleetPulse_BackEndDevelopment.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetPulse_BackEndDevelopment.Data
{
    public class FleetPulseDbContext : DbContext
    {
        public FleetPulseDbContext(DbContextOptions<FleetPulseDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<VerificationCode> VerificationCodes { get; set; }
        public DbSet<FCMNotification> FCMNotifications { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }
        public DbSet<Manufacture> Manufactures { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<FuelRefill> FuelRefills { get; set; }
        public DbSet<Accident> Accidents { get; set; }
        public DbSet<VehicleMaintenance> VehicleMaintenances { get; set; }
        public DbSet<VehicleMaintenanceType> VehicleMaintenanceTypes { get; set; }
        public DbSet<VehicleMaintenanceConfiguration> VehicleMaintenanceConfigurations { get; set; }
        public DbSet<FuelRefillUser> FuelRefillUsers { get; set; }
        public DbSet<AccidentPhoto> AccidentPhotos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new VehicleTypeConfig());
            modelBuilder.ApplyConfiguration(new ManufactureConfig());
            modelBuilder.ApplyConfiguration(new FuelRefillConfig());
            modelBuilder.ApplyConfiguration(new VehicleConfig());
            modelBuilder.ApplyConfiguration(new AccidentConfig());
            modelBuilder.ApplyConfiguration(new VehicleMaintenanceConfig());
            modelBuilder.ApplyConfiguration(new VehicleMaintenanceTypeConfig());
            modelBuilder.ApplyConfiguration(new VerificationCodeConfig());
            modelBuilder.ApplyConfiguration(new FCMNotificationConfig());
            modelBuilder.ApplyConfiguration(new TripConfig());
            modelBuilder.ApplyConfiguration(new FuelRefillUserConfig());
            modelBuilder.ApplyConfiguration(new UserConfig()); 
            modelBuilder.ApplyConfiguration(new AccidentPhotoConfig());
        }
    }
}