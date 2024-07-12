using FleetPulse_BackEndDevelopment.Data;
using FleetPulse_BackEndDevelopment.Data.DTO;
using FleetPulse_BackEndDevelopment.Models;
using FleetPulse_BackEndDevelopment.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FleetPulse_BackEndDevelopment.Services
{
    public class VehicleMaintenanceService : IVehicleMaintenanceService
    {
        private readonly FleetPulseDbContext _context;

        public VehicleMaintenanceService(FleetPulseDbContext context)
        {
            _context = context;
        }

        public async Task<List<VehicleMaintenanceDTO>> GetAllVehicleMaintenancesAsync()
        {
            return await _context.VehicleMaintenances
                .Select(vm => new VehicleMaintenanceDTO
                {
                    MaintenanceId = vm.MaintenanceId,
                    MaintenanceDate = vm.MaintenanceDate,
                    Cost = vm.Cost,
                    PartsReplaced = vm.PartsReplaced,
                    ServiceProvider = vm.ServiceProvider,
                    SpecialNotes = vm.SpecialNotes,
                    VehicleId = vm.VehicleId,
                    VehicleMaintenanceTypeId = vm.VehicleMaintenanceTypeId,
                    Status = vm.Status,
                    VehicleRegistrationNo = _context.Vehicles
                        .Where(v => v.VehicleId == vm.VehicleId)
                        .Select(v => v.VehicleRegistrationNo)
                        .FirstOrDefault(),
                    TypeName = _context.VehicleMaintenanceTypes
                        .Where(mt => mt.Id == vm.VehicleMaintenanceTypeId)
                        .Select(mt => mt.TypeName)
                        .FirstOrDefault()
                }).ToListAsync();
        }

        public async Task<VehicleMaintenance> GetVehicleMaintenanceByIdAsync(int maintenanceId)
        {
            return await _context.VehicleMaintenances.FindAsync(maintenanceId);
        }

        public async Task<VehicleMaintenance> AddVehicleMaintenanceAsync(VehicleMaintenance maintenance)
        {
            try
            {
                var vehicleExists = await _context.Vehicles.AnyAsync(v => v.VehicleId == maintenance.VehicleId);
                var maintenanceTypeExists = await _context.VehicleMaintenanceTypes.AnyAsync(mt => mt.Id == maintenance.VehicleMaintenanceTypeId);

                if (!vehicleExists)
                {
                    throw new Exception("Invalid VehicleId.");
                }

                if (!maintenanceTypeExists)
                {
                    throw new Exception("Invalid VehicleMaintenanceTypeId.");
                }

                _context.VehicleMaintenances.Add(maintenance);
                await _context.SaveChangesAsync();
                return maintenance;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"An error occurred while adding the vehicle maintenance: {ex.Message}");
                throw new Exception("An error occurred while adding the vehicle maintenance.", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                throw new Exception("An unexpected error occurred while adding the vehicle maintenance.", ex);
            }
        }

        public async Task<Vehicle> GetByRegNoAsync(string regNo)
        {
            return await _context.Vehicles.FirstOrDefaultAsync(c => c.VehicleRegistrationNo == regNo);
        }

        public async Task<bool> IsVehicleMaintenanceExistAsync(int id)
        {
            return await _context.VehicleMaintenances.AnyAsync(x => x.MaintenanceId == id);
        }

        public async Task<VehicleMaintenance> UpdateVehicleMaintenanceAsync(int id, VehicleMaintenance maintenance)
        {
            var existingMaintenance = await _context.VehicleMaintenances.FindAsync(id);
            if (existingMaintenance == null)
                throw new KeyNotFoundException("Vehicle Maintenance not found");

            existingMaintenance.MaintenanceDate = maintenance.MaintenanceDate;
            existingMaintenance.Cost = maintenance.Cost;
            existingMaintenance.PartsReplaced = maintenance.PartsReplaced;
            existingMaintenance.ServiceProvider = maintenance.ServiceProvider;
            existingMaintenance.SpecialNotes = maintenance.SpecialNotes;
            existingMaintenance.VehicleId = maintenance.VehicleId;
            existingMaintenance.VehicleMaintenanceTypeId = maintenance.VehicleMaintenanceTypeId;
            existingMaintenance.Status = maintenance.Status;

            _context.VehicleMaintenances.Update(existingMaintenance);
            await _context.SaveChangesAsync();

            return existingMaintenance;
        }

        public async Task DeactivateMaintenanceAsync(int maintenanceId)
        {
            var maintenance = await _context.VehicleMaintenances.FindAsync(maintenanceId);

            if (maintenance == null)
            {
                throw new InvalidOperationException("Maintenance not found.");
            }

            if (MaintenanceIsAssociatedWithVehicle(maintenance))
            {
                throw new InvalidOperationException("Maintenance is associated with a vehicle. Cannot deactivate.");
            }

            maintenance.Status = false;
            await _context.SaveChangesAsync();
        }

        private bool MaintenanceIsAssociatedWithVehicle(VehicleMaintenance maintenance)
        {
            return _context.Vehicles.Any(v => v.VehicleMaintenances.Any(vm => vm.MaintenanceId == maintenance.MaintenanceId));
        }

        public async Task<string> GetRegNoByVehicleIdAsync(int vehicleId)
        {
            return await _context.Vehicles
                .Where(v => v.VehicleId == vehicleId)
                .Select(v => v.VehicleRegistrationNo)
                .FirstOrDefaultAsync();
        }

        public async Task<string> GetTypeNameByMaintenanceTypeIdAsync(int maintenanceTypeId)
        {
            return await _context.VehicleMaintenanceTypes
                .Where(mt => mt.Id == maintenanceTypeId)
                .Select(mt => mt.TypeName)
                .FirstOrDefaultAsync();
        }
    }
}
