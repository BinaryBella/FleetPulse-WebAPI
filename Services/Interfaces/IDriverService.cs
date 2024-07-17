using FleetPulse_BackEndDevelopment.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FleetPulse_BackEndDevelopment.Services.Interfaces
{
    public interface IDriverService
    {
        Task<IEnumerable<DriverDTO>> GetAllDriversAsync();
        Task<DriverDTO> GetDriverByIdAsync(int id);
        Task<DriverDTO> CreateDriverAsync(DriverDTO driverDto);
        Task<DriverDTO> UpdateDriverAsync(int id, DriverDTO driverDto);
        Task<bool> DeactivateDriverAsync(int id);
        Task<bool> ActivateDriverAsync(int id);
        Task<bool> DoesEmailExistAsync(string email);
        Task<bool> DoesUsernameExistAsync(string username);
        Task<int> GetDriverCountAsync();
    }
}