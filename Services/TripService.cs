using FleetPulse_BackEndDevelopment.Data;
using FleetPulse_BackEndDevelopment.Data.DTO;
using FleetPulse_BackEndDevelopment.Models;
using FleetPulse_BackEndDevelopment.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FleetPulse_BackEndDevelopment.Services
{
    public class TripService : ITripService
    {
        private readonly FleetPulseDbContext _context;

        public TripService(FleetPulseDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetDailyTripCountAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Trips.CountAsync(t => t.Date.Date == today);
        }
        
        public async Task<IEnumerable<TripDTO>> GetAllTripsAsync()
        {
            return await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.User)
                .Select(t => new TripDTO
                {
                    TripId = t.TripId,
                    Date = t.Date,
                    StartTime = t.StartTime,
                    EndTime = t.EndTime,
                    StartMeterValue = t.StartMeterValue,
                    EndMeterValue = t.EndMeterValue,
                    VehicleRegistrationNo = t.Vehicle.VehicleRegistrationNo,
                    NIC = t.User.NIC,
                    Status = t.Status
                })
                .ToListAsync();
        }

        public async Task<Trip> GetTripByIdAsync(int id)
        {
            return await _context.Trips.FindAsync(id);
        }

        public async Task<bool> IsTripExist(int id)
        {
            return await _context.Trips.AnyAsync(x => x.TripId == id);
        }
        public bool DoesTripExists(int tripId)
        {
            return _context.Trips.Any(x => x.TripId == tripId);
        }

        public async Task<Trip> AddTripAsync(TripDTO tripDto)
        {
            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.VehicleRegistrationNo == tripDto.VehicleRegistrationNo);
            if (vehicle == null)
            {
                throw new Exception("Vehicle not found.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.NIC == tripDto.NIC);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            var trip = new Trip
            {
                Date = tripDto.Date,
                StartTime = tripDto.StartTime,
                EndTime = tripDto.EndTime,
                StartMeterValue = tripDto.StartMeterValue,
                EndMeterValue = tripDto.EndMeterValue,
                Status = tripDto.Status,
                VehicleId = vehicle.VehicleId,
                UserId = user.UserId
            };

            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            return trip;
        }

        public async Task<bool> UpdateTripAsync(Trip trip)
        {
            try
            {
                _context.Trips.Update(trip);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ActivateTripAsync(int tripId)
        {
            var trip = await _context.Vehicles.FindAsync(tripId);
            if (trip == null) return false;

            trip.Status = true;
            _context.Vehicles.Update(trip);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeactivateTripAsync(int tripId)
        {
            var trip = await _context.Vehicles.FindAsync(tripId);
            if (trip == null) return false;

            trip.Status = false;
            _context.Vehicles.Update(trip);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
