using FleetPulse_BackEndDevelopment.Data;
using FleetPulse_BackEndDevelopment.DTOs;
using FleetPulse_BackEndDevelopment.Models;
using FleetPulse_BackEndDevelopment.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FleetPulse_BackEndDevelopment.Services
{
    public class DriverService : IDriverService
    {
        private readonly FleetPulseDbContext _context;
        private readonly IEmailUserCredentialService _emailService;
        public DriverService(FleetPulseDbContext context, IEmailUserCredentialService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IEnumerable<DriverDTO>> GetAllDriversAsync()
        {
            return await _context.Users
                .Where(u => u.JobTitle == "Driver")
                .Select(u => new DriverDTO
                {
                    UserId = u.UserId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    DateOfBirth = u.DateOfBirth,
                    NIC = u.NIC,
                    DriverLicenseNo = u.DriverLicenseNo,
                    LicenseExpiryDate = u.LicenseExpiryDate,
                    EmailAddress = u.EmailAddress,
                    PhoneNo = u.PhoneNo,
                    EmergencyContact = u.EmergencyContact,
                    BloodGroup = u.BloodGroup,
                    Status = u.Status,
                    UserName = u.UserName,
                    Password = u.HashedPassword 
                }).ToListAsync();
        }

        public async Task<DriverDTO> GetDriverByIdAsync(int id)
        {
            var user = await _context.Users
                .Where(u => u.JobTitle == "Driver" && u.UserId == id)
                .FirstOrDefaultAsync();

            if (user == null)
                return null;

            return new DriverDTO
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                NIC = user.NIC,
                DriverLicenseNo = user.DriverLicenseNo,
                LicenseExpiryDate = user.LicenseExpiryDate,
                EmailAddress = user.EmailAddress,
                PhoneNo = user.PhoneNo,
                EmergencyContact = user.EmergencyContact,
                BloodGroup = user.BloodGroup,
                Status = user.Status,
                UserName = user.UserName,
                Password = user.HashedPassword
            };
        }

        public async Task<User?> GetDriverByNIC(string nic)
        {
            var user = await _context.Users
                .Where(u => u.JobTitle == "Driver" && u.NIC == nic)
                .FirstOrDefaultAsync();
            
            return user ?? null;
        }

        public async Task<DriverDTO> CreateDriverAsync(DriverDTO driverDto)
        {
            if (await DoesEmailExistAsync(driverDto.EmailAddress))
                throw new Exception("Email already exists");
            if (await DoesUsernameExistAsync(driverDto.UserName))
                throw new Exception("Username already exists");

            // Hash the password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(driverDto.Password);

            var user = new User
            {
                FirstName = driverDto.FirstName,
                LastName = driverDto.LastName,
                DateOfBirth = driverDto.DateOfBirth,
                NIC = driverDto.NIC,
                DriverLicenseNo = driverDto.DriverLicenseNo,
                LicenseExpiryDate = driverDto.LicenseExpiryDate,
                EmailAddress = driverDto.EmailAddress,
                PhoneNo = driverDto.PhoneNo,
                EmergencyContact = driverDto.EmergencyContact,
                BloodGroup = driverDto.BloodGroup,
                Status = driverDto.Status,
                UserName = driverDto.UserName,
                HashedPassword = hashedPassword,
                JobTitle = "Driver"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            await _emailService.SendUsernameAndPassword(user.EmailAddress, user.UserName, driverDto.Password);
            driverDto.UserId = user.UserId;
            driverDto.Password = null;
            return driverDto;
        }

        public async Task<DriverDTO> UpdateDriverAsync(int id, DriverDTO driverDto)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null || user.JobTitle != "Driver")
                return null;

            user.FirstName = driverDto.FirstName;
            user.LastName = driverDto.LastName;
            user.DateOfBirth = driverDto.DateOfBirth;
            user.NIC = driverDto.NIC;
            user.DriverLicenseNo = driverDto.DriverLicenseNo;
            user.LicenseExpiryDate = driverDto.LicenseExpiryDate;
            user.EmailAddress = driverDto.EmailAddress;
            user.PhoneNo = driverDto.PhoneNo;
            user.EmergencyContact = driverDto.EmergencyContact;
            user.BloodGroup = driverDto.BloodGroup;
            user.Status = driverDto.Status;
            user.UserName = driverDto.UserName;
            user.HashedPassword = driverDto.Password; // Ideally should hash the password

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return driverDto;
        }

        public async Task<bool> DeactivateDriverAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null || user.JobTitle != "Driver")
                return false;

            user.Status = false;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ActivateDriverAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null || user.JobTitle != "Driver")
                return false;

            user.Status = true;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DoesEmailExistAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.EmailAddress == email);
        }

        public async Task<bool> DoesUsernameExistAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.UserName == username);
        }
        
        public async Task<int> GetDriverCountAsync()
        {
            return _context.Users.Count(u => u.JobTitle == "Driver");
        }
    }
}
