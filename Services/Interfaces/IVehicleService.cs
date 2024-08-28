using FleetPulse_BackEndDevelopment.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using FleetPulse_BackEndDevelopment.Models;

namespace FleetPulse_BackEndDevelopment.Services
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleDTO>> GetAllVehiclesAsync();
        Task<VehicleDTO> GetVehicleByIdAsync(int vehicleId);
        Task<VehicleDTO> AddVehicleAsync(VehicleDTO vehicleDto);
        Task<VehicleDTO> UpdateVehicleAsync(int vehicleId, VehicleDTO vehicleDto);
        Task<bool> ActivateVehicleAsync(int vehicleId);
        Task<bool> DeactivateVehicleAsync(int vehicleId);
        Task<bool> IsRegistrationNoUniqueAsync(string vehicleRegistrationNo);
        Task<int> GetVehicleCountAsync();
        Task<Vehicle?> GetVehicleByRegNumber(string regNumber);
    }
}