using System.ComponentModel.DataAnnotations;

namespace FleetPulse_BackEndDevelopment.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; }
    }
}