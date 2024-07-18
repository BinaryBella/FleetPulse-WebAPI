namespace FleetPulse_BackEndDevelopment.DTOs
{
    public class VehicleDTO
    {
        public int VehicleId { get; set; }
        public string VehicleRegistrationNo { get; set; }
        public string LicenseNo { get; set; }
        public DateTime LicenseExpireDate { get; set; }
        public string? VehicleColor { get; set; }
        public bool Status { get; set; }
        
        public string FuelType { get; set; }
        public string VehicleType { get; set; }
        public string Manufacturer { get; set; }
    }
}