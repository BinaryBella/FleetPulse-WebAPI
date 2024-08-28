using System.ComponentModel.DataAnnotations;

namespace FleetPulse_BackEndDevelopment.Models
{
    public class Accident
    {
        [Key]
        public int AccidentId { get; set; }
        public string Venue { get; set; }
        public DateTime DateTime { get; set; }
        public byte[] Photos { get; set; }
        public string SpecialNotes { get; set; }
        public Decimal Loss { get; set; }
        public bool DriverInjuredStatus { get; set; }
        public bool HelperInjuredStatus { get; set; }
        public bool VehicleDamagedStatus { get; set; }
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public bool Status { get; set; }
        
        public virtual ICollection<AccidentPhoto> AccidentPhotos { get; set; }
    }
}



