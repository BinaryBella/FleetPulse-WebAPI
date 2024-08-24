using FleetPulse_BackEndDevelopment.Data;
using FleetPulse_BackEndDevelopment.Models;
using FleetPulse_BackEndDevelopment.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FleetPulse_BackEndDevelopment.Services
{
    public class ManufactureService : IManufactureService
    {
        private readonly FleetPulseDbContext _context;

        public ManufactureService(FleetPulseDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Manufacture?>> GetAllManufacturesAsync() // Corrected name
        {
            return await _context.Manufactures.ToListAsync();
        }

        public async Task<Manufacture?> GetManufactureByIdAsync(int id)
        {
            return await _context.Manufactures.FindAsync(id);
        }

        public Task<bool> IsManufactureExist(int id)
        {
            return Task.FromResult(_context.Manufactures.Any(x => x.ManufactureId == id));
        }

        public bool DoesManufactureExists(string manufacturer)
        {
            return _context.Manufactures.Any(x => x.Manufacturer == manufacturer);
        }

        public async Task<Manufacture?> AddManufactureAsync(Manufacture? manufacture)
        {
            if (manufacture != null)
            {
                _context.Manufactures.Add(manufacture);
                await _context.SaveChangesAsync();
            }
            return manufacture;
        }

        public async Task<bool> UpdateManufactureAsync(int manufactureId, Manufacture manufacture)
        {
            try
            {
                // Find the existing manufacture by ID
                var existingManufacture = await _context.Manufactures.FindAsync(manufactureId);

                if (existingManufacture == null)
                {
                    return false;
                }

                // Update the existing manufacture's properties
                _context.Entry(existingManufacture).CurrentValues.SetValues(manufacture);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task ActivateManufactureAsync(int ManufactureId)
        {
            var manufacture = await _context.Manufactures.FindAsync(ManufactureId);
            if (manufacture == null)
            {
                throw new KeyNotFoundException("Manufacture not found.");
            }

            manufacture.Status = true;
            await _context.SaveChangesAsync();
        }

        public async Task DeactivateManufactureAsync(int ManufactureId)
        {
            var user = await _context.Manufactures.FindAsync(ManufactureId);

            if (user == null)
            {
                throw new InvalidOperationException("Manufacture not found.");
            }
            else
            {
                user.Status = false;
                await _context.SaveChangesAsync();
            }
        }
    }

}

