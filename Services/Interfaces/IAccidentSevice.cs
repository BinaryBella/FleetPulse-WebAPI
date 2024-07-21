using FleetPulse_BackEndDevelopment.DTOs;

namespace FleetPulse_BackEndDevelopment.Services
{
    public interface IAccidentService
    {
        Task<IEnumerable<AccidentDTO>> GetAllAccidentsAsync();
        Task<AccidentDTO> GetAccidentByIdAsync(int id);
        Task<AccidentDTO> CreateAccidentAsync(AccidentCreateDTO accidentCreateDto);
        Task<AccidentDTO> UpdateAccidentAsync(int id, AccidentDTO accidentDto);
        Task<bool> DeactivateAccidentAsync(int id);
        Task<bool> ActivateAccidentAsync(int id);
        int GetLatestMonthAccidentCount();
    }
}