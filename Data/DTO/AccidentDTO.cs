namespace FleetPulse_BackEndDevelopment.DTOs
{
    public class AccidentDTO
    {
        public int AccidentId { get; set; }
        public string Venue { get; set; }
        public DateTime DateTime { get; set; }
        public string SpecialNotes { get; set; }
        public Decimal Loss { get; set; }
        public bool DriverInjuredStatus { get; set; }
        public bool HelperInjuredStatus { get; set; }
        public bool VehicleDamagedStatus { get; set; }
        public int VehicleId { get; set; }
        public bool Status { get; set; }
        public List<IFormFile> Photos { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public byte[] StoredPhotos { get; set; } 
    }
}