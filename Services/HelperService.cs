using FleetPulse_BackEndDevelopment.Data;
using FleetPulse_BackEndDevelopment.DTOs;
using FleetPulse_BackEndDevelopment.Models;
using FleetPulse_BackEndDevelopment.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FleetPulse_BackEndDevelopment.Services
{
    public class HelperService : IHelperService
    {
        private readonly FleetPulseDbContext _context;
        private readonly IEmailUserCredentialService _emailService;

        public HelperService(FleetPulseDbContext context, IEmailUserCredentialService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IEnumerable<HelperDTO>> GetAllHelpersAsync()
        {
            return await _context.Users
                .Where(u => u.JobTitle == "Helper")
                .Select(u => new HelperDTO
                {
                    UserId = u.UserId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    DateOfBirth = u.DateOfBirth,
                    NIC = u.NIC,
                    EmailAddress = u.EmailAddress,
                    PhoneNo = u.PhoneNo,
                    EmergencyContact = u.EmergencyContact,
                    BloodGroup = u.BloodGroup,
                    Status = u.Status,
                    UserName = u.UserName,
                    Password = u.HashedPassword // Assuming hashed password is already stored
                }).ToListAsync();
        }

        public async Task<HelperDTO> GetHelperByIdAsync(int id)
        {
            var user = await _context.Users
                .Where(u => u.JobTitle == "Helper" && u.UserId == id)
                .FirstOrDefaultAsync();

            if (user == null)
                return null;

            return new HelperDTO
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                NIC = user.NIC,
                EmailAddress = user.EmailAddress,
                PhoneNo = user.PhoneNo,
                EmergencyContact = user.EmergencyContact,
                BloodGroup = user.BloodGroup,
                Status = user.Status,
                UserName = user.UserName,
                Password = user.HashedPassword
            };
        }

        public async Task<HelperDTO> CreateHelperAsync(HelperDTO helperDto)
        {
            // Check if email exists
            if (await DoesEmailExistAsync(helperDto.EmailAddress))
                throw new Exception("Email already exists");

            // Check if username exists
            if (await DoesUsernameExistAsync(helperDto.UserName))
                throw new Exception("Username already exists");

            var user = new User
            {
                FirstName = helperDto.FirstName,
                LastName = helperDto.LastName,
                DateOfBirth = helperDto.DateOfBirth,
                NIC = helperDto.NIC,
                EmailAddress = helperDto.EmailAddress,
                PhoneNo = helperDto.PhoneNo,
                EmergencyContact = helperDto.EmergencyContact,
                BloodGroup = helperDto.BloodGroup,
                Status = helperDto.Status,
                UserName = helperDto.UserName,
                HashedPassword = helperDto.Password, // Ideally should hash the password
                JobTitle = "Helper"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            await _emailService.SendUsernameAndPassword(user.EmailAddress, user.UserName, helperDto.Password);
            helperDto.UserId = user.UserId;
            return helperDto;
        }

        public async Task<HelperDTO> UpdateHelperAsync(int id, HelperDTO helperDto)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null || user.JobTitle != "Helper")
                return null;

            user.FirstName = helperDto.FirstName;
            user.LastName = helperDto.LastName;
            user.DateOfBirth = helperDto.DateOfBirth;
            user.NIC = helperDto.NIC;
            user.EmailAddress = helperDto.EmailAddress;
            user.PhoneNo = helperDto.PhoneNo;
            user.EmergencyContact = helperDto.EmergencyContact;
            user.BloodGroup = helperDto.BloodGroup;
            user.Status = helperDto.Status;
            user.UserName = helperDto.UserName;
            user.HashedPassword = helperDto.Password; // Ideally should hash the password

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return helperDto;
        }

        public async Task<bool> DeactivateHelperAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null || user.JobTitle != "Helper")
                return false;

            user.Status = false;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ActivateHelperAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null || user.JobTitle != "Helper")
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
        
        public async Task<int> GetHelperCountAsync()
        {
            return _context.Users.Count(u => u.JobTitle == "Helper");
        }
    }
}
