using FleetPulse_BackEndDevelopment.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FleetPulse_BackEndDevelopment.Services.Interfaces
{
    public interface IHelperService
    {
        Task<IEnumerable<HelperDTO>> GetAllHelpersAsync();
        Task<HelperDTO> GetHelperByIdAsync(int id);
        Task<HelperDTO> CreateHelperAsync(HelperDTO helperDto);
        Task<HelperDTO> UpdateHelperAsync(int id, HelperDTO helperDto);
        Task<bool> DeactivateHelperAsync(int id);
        Task<bool> ActivateHelperAsync(int id);
        Task<bool> DoesEmailExistAsync(string email);
        Task<bool> DoesUsernameExistAsync(string username);
    }
}