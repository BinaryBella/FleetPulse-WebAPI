using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace FleetPulse_BackEndDevelopment.DTOs
{
    public class AccidentCreateDTO
    {
        public string Venue { get; set; }
        public DateTime DateTime { get; set; }
        public List<IFormFile> Photos { get; set; }
        public string SpecialNotes { get; set; }
        public decimal Loss { get; set; }
        public bool DriverInjuredStatus { get; set; }
        public bool HelperInjuredStatus { get; set; }
        public bool VehicleDamagedStatus { get; set; }
        public int VehicleId { get; set; }
        public bool Status { get; set; }
    }
}