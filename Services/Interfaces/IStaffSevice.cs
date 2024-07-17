using FleetPulse_BackEndDevelopment.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FleetPulse_BackEndDevelopment.Services.Interfaces
{
    public interface IStaffService
    {
        Task<IEnumerable<StaffDTO>> GetAllStaffAsync();
        Task<StaffDTO> GetStaffByIdAsync(int id);
        Task<StaffDTO> CreateStaffAsync(StaffDTO staffDto);
        Task<StaffDTO> UpdateStaffAsync(int id, StaffDTO staffDto);
        Task<bool> DeactivateStaffAsync(int id);
        Task<bool> ActivateStaffAsync(int id);
        Task<bool> DoesEmailExistAsync(string email);
        Task<bool> DoesUsernameExistAsync(string username);
    }
}