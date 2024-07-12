public class VehicleDTO
{
    public int VehicleId { get; set; }
    public string VehicleRegistrationNo { get; set; }
    public string? LicenseNo { get; set; }
    public DateTime LicenseExpireDate { get; set; }
    public string? VehicleColor { get; set; }
    public string? Status { get; set; }
    public int VehicleTypeId { get; set; }
    public int ManufactureId { get; set; }
    public List<int>? VehicleMaintenanceIds { get; set; }
    public List<int>? FuelRefillIds { get; set; }
}