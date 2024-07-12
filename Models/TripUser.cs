namespace FleetPulse_BackEndDevelopment.Models
{
    public class TripUser
    {
        public int TripId { get; set; }
        public Trip Trip { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }
    }
}