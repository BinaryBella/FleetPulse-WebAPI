namespace FleetPulse_BackEndDevelopment.DTOs
{
    public class AccidentDTO
    {
        public int AccidentId { get; set; }
        public string Venue { get; set; }
        public DateTime DateTime { get; set; }
        public string SpecialNotes { get; set; }
        public decimal Loss { get; set; }
        public bool DriverInjuredStatus { get; set; }
        public bool HelperInjuredStatus { get; set; }
        public bool VehicleDamagedStatus { get; set; }
        public string VehicleRegistrationNo { get; set; }
        public string NIC { get; set; }
        public bool Status { get; set; }
    }
}