using System.ComponentModel.DataAnnotations;

namespace FleetPulse_BackEndDevelopment.Models;

public class AccidentPhoto
{
    [Key]
    public int Id { get; set; }
    public int AccidentId { get; set; }
    public Accident Accident { get; set; }
    public byte[] Photo { get; set; }
}