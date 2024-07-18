using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetPulse_BackEndDevelopment.Models
{
    public class Vehicle
    {
        [Key]
        public int VehicleId { get; set; }
        public string VehicleRegistrationNo { get; set; }
        public string LicenseNo { get; set; }
        public DateTime LicenseExpireDate { get; set; }
        public string? VehicleColor { get; set; }
        public string FuelType { get; set; }
        public bool Status { get; set; }
        public int VehicleTypeId { get; set; }
        public VehicleType Type { get; set; }
        public int ManufactureId { get; set; }
        public Manufacture Manufacturer { get; set; }
        public ICollection<Trip> Trips { get; set; }
        public ICollection<Accident> Accidents { get; set; }
        public ICollection<VehicleMaintenance> VehicleMaintenances { get; set; }
        public ICollection<FuelRefill> FuelRefills { get; set; }
    }
}