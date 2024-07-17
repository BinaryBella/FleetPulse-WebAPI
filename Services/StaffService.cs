using FleetPulse_BackEndDevelopment.Data;
using FleetPulse_BackEndDevelopment.DTOs;
using FleetPulse_BackEndDevelopment.Models;
using FleetPulse_BackEndDevelopment.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FleetPulse_BackEndDevelopment.Services
{
    public class StaffService : IStaffService
    {
        private readonly FleetPulseDbContext _context;
        private readonly IEmailUserCredentialService _emailService;

        public StaffService(FleetPulseDbContext context, IEmailUserCredentialService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IEnumerable<StaffDTO>> GetAllStaffAsync()
        {
            return await _context.Users
                .Where(u => u.JobTitle == "Staff")
                .Select(u => new StaffDTO
                {
                    UserId = u.UserId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    DateOfBirth = u.DateOfBirth,
                    NIC = u.NIC,
                    EmailAddress = u.EmailAddress,
                    PhoneNo = u.PhoneNo,
                    EmergencyContact = u.EmergencyContact,
                    Status = u.Status,
                    UserName = u.UserName,
                    Password = u.HashedPassword // Assuming hashed password is already stored
                }).ToListAsync();
        }

        public async Task<StaffDTO> GetStaffByIdAsync(int id)
        {
            var user = await _context.Users
                .Where(u => u.JobTitle == "Staff" && u.UserId == id)
                .FirstOrDefaultAsync();

            if (user == null)
                return null;

            return new StaffDTO
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                NIC = user.NIC,
                EmailAddress = user.EmailAddress,
                PhoneNo = user.PhoneNo,
                EmergencyContact = user.EmergencyContact,
                Status = user.Status,
                UserName = user.UserName,
                Password = user.HashedPassword
            };
        }

        public async Task<StaffDTO> CreateStaffAsync(StaffDTO staffDto)
        {
            // Check if email exists
            if (await DoesEmailExistAsync(staffDto.EmailAddress))
                throw new Exception("Email already exists");

            // Check if username exists
            if (await DoesUsernameExistAsync(staffDto.UserName))
                throw new Exception("Username already exists");

            var user = new User
            {
                FirstName = staffDto.FirstName,
                LastName = staffDto.LastName,
                DateOfBirth = staffDto.DateOfBirth,
                NIC = staffDto.NIC,
                EmailAddress = staffDto.EmailAddress,
                PhoneNo = staffDto.PhoneNo,
                EmergencyContact = staffDto.EmergencyContact,
                Status = staffDto.Status,
                UserName = staffDto.UserName,
                HashedPassword = staffDto.Password,
                JobTitle = "Staff"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            await _emailService.SendUsernameAndPassword(user.EmailAddress, user.UserName, staffDto.Password);

            staffDto.UserId = user.UserId;
            return staffDto;
        }

        public async Task<StaffDTO> UpdateStaffAsync(int id, StaffDTO staffDto)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null || user.JobTitle != "Staff")
                return null;

            user.FirstName = staffDto.FirstName;
            user.LastName = staffDto.LastName;
            user.DateOfBirth = staffDto.DateOfBirth;
            user.NIC = staffDto.NIC;
            user.EmailAddress = staffDto.EmailAddress;
            user.PhoneNo = staffDto.PhoneNo;
            user.EmergencyContact = staffDto.EmergencyContact;
            user.Status = staffDto.Status;
            user.UserName = staffDto.UserName;
            user.HashedPassword = staffDto.Password; // Ideally should hash the password

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return staffDto;
        }

        public async Task<bool> DeactivateStaffAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null || user.JobTitle != "Staff")
                return false;

            user.Status = false;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ActivateStaffAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null || user.JobTitle != "Staff")
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
    }
}
