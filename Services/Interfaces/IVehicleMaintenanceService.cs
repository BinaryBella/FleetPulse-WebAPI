using FleetPulse_BackEndDevelopment.Data.DTO;
using FleetPulse_BackEndDevelopment.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FleetPulse_BackEndDevelopment.Services.Interfaces
{
    public interface IVehicleMaintenanceService
    {
        Task<List<VehicleMaintenanceDTO>> GetAllVehicleMaintenancesAsync();
        Task<VehicleMaintenance> GetVehicleMaintenanceByIdAsync(int maintenanceId);
        Task<VehicleMaintenance> AddVehicleMaintenanceAsync(VehicleMaintenance maintenance);
        Task<Vehicle> GetByRegNoAsync(string regNo);
        Task<bool> IsVehicleMaintenanceExistAsync(int id);
        Task<VehicleMaintenance> UpdateVehicleMaintenanceAsync(int id, VehicleMaintenance maintenance);
        Task DeactivateMaintenanceAsync(int maintenanceId);
        Task<string> GetRegNoByVehicleIdAsync(int vehicleId);
        Task<string> GetTypeNameByMaintenanceTypeIdAsync(int maintenanceTypeId);
    }
}