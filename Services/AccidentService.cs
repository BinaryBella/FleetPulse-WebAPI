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
            var accident = _mapper.Map<Accident>(accidentCreateDto);
            var accidentPhotos = new List<AccidentPhoto>();
            
            accident.Photos = Array.Empty<byte>();

            if (accidentCreateDto.SpecialNotes == null)
            {
                accident.SpecialNotes = "";
            }
            
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
            accident.Status = true;
            var result = _context.Accidents.Add(accident);
            await _context.SaveChangesAsync();
            
            if (accidentCreateDto.Photos != null && accidentCreateDto.Photos.Count > 0)
            {
                foreach (var formFile in accidentCreateDto.Photos)
                {
                    var photoBytesList = new List<byte[]>();
                    if (formFile.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        await formFile.CopyToAsync(memoryStream);
                        photoBytesList.Add(memoryStream.ToArray());
                    }
                    
                    var accidentPhoto = new AccidentPhoto
                    {
                        Photo = photoBytesList.Last(),
                        AccidentId = result.Entity.AccidentId
                    };
                    
                    accidentPhotos.Add(accidentPhoto);
                }
            }
            
            _context.AccidentPhotos.AddRange(accidentPhotos);
            
            await _context.SaveChangesAsync();

            return _mapper.Map<AccidentDTO>(accident);
        }


        public async Task<List<AccidentPhoto>> GetAccidentPhotosAsync(int id)
        {
            return await _context.AccidentPhotos.Where(ap => ap.AccidentId == id).ToListAsync();
        }

        public async Task<AccidentDTO?> UpdateAccidentAsync(int id, AccidentDTO accidentDto)
        {
            var accident = await _context.Accidents.Include(a => a.Vehicle).Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AccidentId == id);
            if (accident == null) return null;

            // Find the vehicle based on the registration number
            var vehicle =
                await _context.Vehicles.FirstOrDefaultAsync(v =>
                    v.VehicleRegistrationNo == accidentDto.VehicleRegistrationNo);
            
            if (vehicle == null) return null;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.NIC == accidentDto.NIC);
            if (user == null) return null;

            // Manually set the correct VehicleId
            accident.Venue = accidentDto.Venue;
            accident.DateTime = accidentDto.DateTime;
            accident.SpecialNotes = accidentDto.SpecialNotes;
            accident.Loss = accidentDto.Loss;
            accident.DriverInjuredStatus = accidentDto.DriverInjuredStatus;
            accident.HelperInjuredStatus = accidentDto.HelperInjuredStatus;
            accident.VehicleDamagedStatus = accidentDto.VehicleDamagedStatus;
            accident.VehicleId = vehicle.VehicleId;
            accident.UserId = user.UserId;

            // Manually map other properties that AutoMapper can't handle automatically
            accident.UserId =
                await GetUserIdByNICAsync(accidentDto.NIC); // Assume you have a method to get UserId from NIC

            // Map the rest of the properties

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