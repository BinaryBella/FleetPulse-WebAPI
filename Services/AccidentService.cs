using AutoMapper;
using FleetPulse_BackEndDevelopment.Models;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using FleetPulse_BackEndDevelopment.Data;
using FleetPulse_BackEndDevelopment.DTOs;
using FleetPulse_BackEndDevelopment.Services.Interfaces;

namespace FleetPulse_BackEndDevelopment.Services
{
    public class AccidentService : IAccidentService
    {
        private readonly FleetPulseDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDriverService _driverService;
        private readonly IVehicleService _vehicleService;

        public AccidentService(FleetPulseDbContext context, IMapper mapper, IDriverService driverService,
            IVehicleService vehicleService)
        {
            _context = context;
            _mapper = mapper;
            _driverService = driverService;
            _vehicleService = vehicleService;
        }

        public async Task<IEnumerable<AccidentDTO>> GetAllAccidentsAsync()
        {
            return await _context.Accidents
                .Include(a => a.Vehicle)
                .Include(a => a.User) // Include User to get NIC
                .Select(a => new AccidentDTO
                {
                    AccidentId = a.AccidentId,
                    Venue = a.Venue,
                    DateTime = a.DateTime,
                    SpecialNotes = a.SpecialNotes,
                    Loss = a.Loss,
                    DriverInjuredStatus = a.DriverInjuredStatus,
                    HelperInjuredStatus = a.HelperInjuredStatus,
                    VehicleDamagedStatus = a.VehicleDamagedStatus,
                    VehicleRegistrationNo = a.Vehicle.VehicleRegistrationNo,
                    NIC = a.User.NIC, // Map NIC from User
                    Status = a.Status
                })
                .ToListAsync();
        }

        public async Task<AccidentDTO> GetAccidentByIdAsync(int id)
        {
            var accident = await _context.Accidents
                .Include(a => a.Vehicle)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AccidentId == id);

            if (accident == null)
                return null;

            var accidentDto = _mapper.Map<AccidentDTO>(accident);
            return accidentDto;
        }

        public int GetLatestMonthAccidentCount()
        {
            var currentDate = DateTime.Now;
            var lastMonthDate = currentDate.AddMonths(-1);

            return _context.Accidents
                .Count(a => a.DateTime >= lastMonthDate && a.DateTime <= currentDate);
        }

        public async Task<AccidentDTO> CreateAccidentAsync(AccidentCreateDTO accidentCreateDto)
        {
            // Create a new Accident instance
            var accident = new Accident
            {
                Venue = accidentCreateDto.Venue,
                DateTime = accidentCreateDto.DateTime,
                SpecialNotes = accidentCreateDto.SpecialNotes ?? string.Empty,
                Loss = accidentCreateDto.Loss,
                DriverInjuredStatus = accidentCreateDto.DriverInjuredStatus,
                HelperInjuredStatus = accidentCreateDto.HelperInjuredStatus,
                VehicleDamagedStatus = accidentCreateDto.VehicleDamagedStatus,
                Status = accidentCreateDto.Status
            };

            var driver = await _driverService.GetDriverByNIC(accidentCreateDto.DriverNIC);
            if (driver == null)
            {
                throw new Exception("Driver not found");
            }

            accident.UserId = driver.UserId;

            var vehicle = await _vehicleService.GetVehicleByRegNumber(accidentCreateDto.VehicleRegistrationNumber);
            if (vehicle == null)
            {
                throw new Exception("Vehicle not found");
            }

            accident.VehicleId = vehicle.VehicleId;

            // Add the Accident to the context
            _context.Accidents.Add(accident);
            await _context.SaveChangesAsync(); // Save to get the AccidentId

            // Create a list to hold the AccidentPhoto entries
            var accidentPhotos = new List<AccidentPhoto>();

            if (accidentCreateDto.Photos != null && accidentCreateDto.Photos.Count > 0)
            {
                foreach (var formFile in accidentCreateDto.Photos)
                {
                    if (formFile.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        await formFile.CopyToAsync(memoryStream);

                        var accidentPhoto = new AccidentPhoto
                        {
                            AccidentId = accident.AccidentId, // Foreign key reference
                            Photo = memoryStream.ToArray() // Store the binary data
                        };

                        accidentPhotos.Add(accidentPhoto);
                    }
                }

                // Add all photos to the context
                _context.AccidentPhotos.AddRange(accidentPhotos);
                await _context.SaveChangesAsync();
            }

            // Map back to a DTO if needed
            return new AccidentDTO
            {
                // Fill properties as needed
            };
        }


        public async Task<List<AccidentPhoto>> GetAccidentPhotosAsync(int id)
        {
            return await _context.AccidentPhotos.Where(ap => ap.AccidentId == id).ToListAsync();
        }

        public async Task<AccidentDTO> UpdateAccidentAsync(int id, AccidentDTO accidentDto)
        {
            var accident = await _context.Accidents.Include(a => a.Vehicle).Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AccidentId == id);
            if (accident == null) return null;

            // Find the vehicle based on the registration number
            var vehicle =
                await _context.Vehicles.FirstOrDefaultAsync(v =>
                    v.VehicleRegistrationNo == accidentDto.VehicleRegistrationNo);
            if (vehicle == null)
            {
                throw new ArgumentException("Invalid vehicle registration number.");
            }

            // Manually set the correct VehicleId
            accident.VehicleId = vehicle.VehicleId;

            // Manually map other properties that AutoMapper can't handle automatically
            accident.UserId =
                await GetUserIdByNICAsync(accidentDto.NIC); // Assume you have a method to get UserId from NIC

            // Map the rest of the properties
            _mapper.Map(accidentDto, accident);

            _context.Accidents.Update(accident);
            await _context.SaveChangesAsync();

            return _mapper.Map<AccidentDTO>(accident);
        }


        private async Task<int> GetUserIdByNICAsync(string nic)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.NIC == nic);
            if (user == null)
            {
                throw new ArgumentException("Invalid NIC.");
            }

            return user.UserId;
        }


        public async Task<bool> DeactivateAccidentAsync(int id)
        {
            var accident = await _context.Accidents.FindAsync(id);
            if (accident == null) return false;

            accident.Status = false;
            _context.Accidents.Update(accident);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ActivateAccidentAsync(int id)
        {
            var accident = await _context.Accidents.FindAsync(id);
            if (accident == null) return false;

            accident.Status = true;
            _context.Accidents.Update(accident);
            await _context.SaveChangesAsync();

            return true;
        }

        private byte[] CombinePhotoBytes(List<byte[]> photoBytesList)
        {
            using var memoryStream = new MemoryStream();
            foreach (var photoBytes in photoBytesList)
            {
                memoryStream.Write(photoBytes, 0, photoBytes.Length);
            }

            return memoryStream.ToArray();
        }
    }
}