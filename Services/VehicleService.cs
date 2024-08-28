using FleetPulse_BackEndDevelopment.DTOs;
using FleetPulse_BackEndDevelopment.Models;
using Microsoft.EntityFrameworkCore;
using FleetPulse_BackEndDevelopment.Data;

namespace FleetPulse_BackEndDevelopment.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly FleetPulseDbContext _context;

        public VehicleService(FleetPulseDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VehicleDTO>> GetAllVehiclesAsync()
        {
            return await _context.Vehicles
                .Include(v => v.Type)
                .Include(v => v.Manufacturer)
                .Select(v => new VehicleDTO
                {
                    VehicleId = v.VehicleId,
                    VehicleRegistrationNo = v.VehicleRegistrationNo,
                    LicenseNo = v.LicenseNo,
                    LicenseExpireDate = v.LicenseExpireDate,
                    VehicleColor = v.VehicleColor,
                    FuelType = v.FuelType,
                    Status = v.Status,
                    VehicleType = v.Type.Type,
                    Manufacturer = v.Manufacturer.Manufacturer
                }).ToListAsync();
        }

        public async Task<Vehicle?> GetVehicleByRegNumber(string regNumber)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleRegistrationNo == regNumber);
            return vehicle ?? null;
        }

        public async Task<VehicleDTO> GetVehicleByIdAsync(int vehicleId)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Type)
                .Include(v => v.Manufacturer)
                .FirstOrDefaultAsync(v => v.VehicleId == vehicleId);
            if (vehicle == null) return null;

            return new VehicleDTO
            {
                VehicleId = vehicle.VehicleId,
                VehicleRegistrationNo = vehicle.VehicleRegistrationNo,
                LicenseNo = vehicle.LicenseNo,
                LicenseExpireDate = vehicle.LicenseExpireDate,
                VehicleColor = vehicle.VehicleColor,
                FuelType = vehicle.FuelType,
                Status = vehicle.Status,
                VehicleType = vehicle.Type.Type,
                Manufacturer = vehicle.Manufacturer.Manufacturer
            };
        }
        
        public async Task<VehicleDTO> AddVehicleAsync(VehicleDTO vehicleDto)
        {
            // Check if VehicleRegistrationNo is unique
            var existingVehicleReg = await _context.Vehicles.AnyAsync(v => v.VehicleRegistrationNo == vehicleDto.VehicleRegistrationNo);
            if (existingVehicleReg)
            {
                throw new ArgumentException($"Vehicle registration number '{vehicleDto.VehicleRegistrationNo}' already exists.");
            }

            // Check if LicenseNo is unique
            var existingLicense = await _context.Vehicles.AnyAsync(v => v.LicenseNo == vehicleDto.LicenseNo);
            if (existingLicense)
            {
                throw new ArgumentException($"License number '{vehicleDto.LicenseNo}' already exists.");
            }

            // Find the VehicleType by Type name
            var vehicleType = await _context.VehicleTypes.FirstOrDefaultAsync(vt => vt.Type == vehicleDto.VehicleType);
            if (vehicleType == null)
            {
                throw new ArgumentException($"Vehicle type '{vehicleDto.VehicleType}' not found.");
            }

            // Find the Manufacture by Manufacturer name
            var manufacture = await _context.Manufactures.FirstOrDefaultAsync(m => m.Manufacturer == vehicleDto.Manufacturer);
            if (manufacture == null)
            {
                throw new ArgumentException($"Manufacturer '{vehicleDto.Manufacturer}' not found.");
            }

            // Create new Vehicle object
            var vehicle = new Vehicle
            {
                VehicleRegistrationNo = vehicleDto.VehicleRegistrationNo,
                LicenseNo = vehicleDto.LicenseNo,
                LicenseExpireDate = vehicleDto.LicenseExpireDate,
                VehicleColor = vehicleDto.VehicleColor,
                FuelType = vehicleDto.FuelType,
                Status = vehicleDto.Status,
                VehicleTypeId = vehicleType.VehicleTypeId,
                ManufactureId = manufacture.ManufactureId
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            vehicleDto.VehicleId = vehicle.VehicleId;
            return vehicleDto;
        }
        
        public async Task<VehicleDTO> UpdateVehicleAsync(int vehicleId, VehicleDTO vehicleDto)
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null) return null;

            vehicle.VehicleRegistrationNo = vehicleDto.VehicleRegistrationNo;
            vehicle.LicenseNo = vehicleDto.LicenseNo;
            vehicle.LicenseExpireDate = vehicleDto.LicenseExpireDate;
            vehicle.VehicleColor = vehicleDto.VehicleColor;
            vehicle.FuelType = vehicleDto.FuelType;
            vehicle.Status = vehicleDto.Status;
            vehicle.VehicleTypeId = _context.VehicleTypes.First(vt => vt.Type == vehicleDto.VehicleType).VehicleTypeId;
            vehicle.ManufactureId = _context.Manufactures.First(m => m.Manufacturer == vehicleDto.Manufacturer).ManufactureId;

            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();

            return vehicleDto;
        }

        public async Task<bool> ActivateVehicleAsync(int vehicleId)
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null) return false;

            vehicle.Status = true;
            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeactivateVehicleAsync(int vehicleId)
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null) return false;

            vehicle.Status = false;
            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsRegistrationNoUniqueAsync(string vehicleRegistrationNo)
        {
            return !await _context.Vehicles.AnyAsync(v => v.VehicleRegistrationNo == vehicleRegistrationNo);
        }
        
        public async Task<int> GetVehicleCountAsync()
        {
            return await _context.Vehicles.CountAsync();
        }
    }
}
