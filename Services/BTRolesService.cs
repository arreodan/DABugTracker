﻿using DABugTracker.Data;
using DABugTracker.Models;
using DABugTracker.Models.Enums;
using DABugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DABugTracker.Services
{
    public class BTRolesService : IBTRolesService
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly ApplicationDbContext _context;

        public BTRolesService(UserManager<BTUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<bool> AddUserToRoleAsync(BTUser user, string roleName)
        {
            try
            {
                bool result = (await _userManager.AddToRoleAsync(user, roleName)).Succeeded;

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<IdentityRole>> GetRolesAsync()
        {
            try
            {
               return await _context.Roles.Where(r => r.Name != nameof(BTRoles.DemoUser)).ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(BTUser user)
        {
            try
            {
                return await _userManager.GetRolesAsync(user);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId)
        {
            List<BTUser> usersInRole = (await _userManager.GetUsersInRoleAsync(roleName)).ToList();

            usersInRole = usersInRole.Where(u => u.CompanyId == companyId)
                              .ToList();
            return usersInRole;
        }

        public async Task<bool> IsUserInRole(BTUser member, string roleName)
        {
            try
            {
                return await _userManager.IsInRoleAsync(member, roleName);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName)
        {
            try
            {
                bool result = (await _userManager.RemoveFromRoleAsync(user, roleName)).Succeeded;

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveUserFromRolesAsync(BTUser user, IEnumerable<string> roleNames)
        {
            try
            {
                bool result = (await _userManager.RemoveFromRolesAsync(user, roleNames)).Succeeded;

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
