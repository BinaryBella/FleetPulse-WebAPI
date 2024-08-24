using FleetPulse_BackEndDevelopment.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using FleetPulse_BackEndDevelopment.Data.DTO;

namespace FleetPulse_BackEndDevelopment.Services.Interfaces
{
    public interface ITripService
    {
        Task<IEnumerable<TripDTO>> GetAllTripsAsync();
        Task<Trip> GetTripByIdAsync(int id);
        Task<bool> IsTripExist(int id);
        bool DoesTripExists(int tripId);
        Task<Trip> AddTripAsync(TripDTO tripDto);
        Task<bool> UpdateTripAsync(Trip trip);
        Task<bool> ActivateTripAsync(int tripId);
        Task<bool> DeactivateTripAsync(int tripId);
        Task<int> GetDailyTripCountAsync();
    }
}