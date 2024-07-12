using FleetPulse_BackEndDevelopment.Data;
using FleetPulse_BackEndDevelopment.Models;
using FleetPulse_BackEndDevelopment.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FleetPulse_BackEndDevelopment.Services
{
    public class StaffService : IStaffService
    {
        private readonly FleetPulseDbContext _context;

        public StaffService(FleetPulseDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllStaffsAsync()
        {
            return await _context.Users
                         .Where(x => x.JobTitle != null &&
                                     x.JobTitle.ToLower() != "driver" &&
                                     x.JobTitle.ToLower() != "helper")
                         .ToListAsync();
        }

        public async Task<User> GetStaffByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public Task<bool> IsStaffExist(int id)
        {
            return Task.FromResult(_context.Users.Any(x => x.UserId == id));
        }

        public bool DoesStaffExist(string NIC)
        {
            return _context.Users.Any(x => x.NIC == NIC);
        }

        public async Task<User> AddStaffAsync(User staff)
        {
            _context.Users.Add(staff);
            await _context.SaveChangesAsync();

            return staff;
        }

        public async Task<bool> UpdateStaffAsync(User staff)
        {
            try
            {
                _context.Users.Update(staff);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task ActivateStaffAsync(int id)
        {
            var driver = await _context.Users.FindAsync(id);
            if (driver == null)
            {
                throw new KeyNotFoundException("Staff not found.");
            }

            driver.Status = true;
            await _context.SaveChangesAsync();
        }

        public async Task DeactivateStaffAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException("Staff not found.");
            }
            else
            {
                user.Status = false;
                await _context.SaveChangesAsync();
            }
        }
    }

}
