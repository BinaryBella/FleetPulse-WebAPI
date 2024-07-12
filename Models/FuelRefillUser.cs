using FleetPulse_BackEndDevelopment.Models;

public class FuelRefillUser
{
    public int UserId { get; set; }
    public User User { get; set; }
    
    public int FuelRefillId { get; set; }
    public FuelRefill FuelRefill { get; set; }
}