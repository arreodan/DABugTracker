using DABugTracker.Data;
using DABugTracker.Models;
using DABugTracker.Services.Interfaces;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.EntityFrameworkCore;

namespace DABugTracker.Services
{
    public class BTInviteService : IBTInviteService
    {
        private readonly ApplicationDbContext _context;

        public BTInviteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AcceptInviteAsync(Guid? token, string userId, int companyId)
        {
            try
            {
                Invite? invite = await _context.Invites
                                               .FirstOrDefaultAsync(i => i.CompanyToken == token 
                                                                                        && i.CompanyId == companyId
                                                                                        && i.IsValid == true);


                if (invite == null)
                {
                    Console.WriteLine();
                    return false;
                }

                invite.IsValid = false;
                invite.InviteeId = userId;
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        public async Task AddnewInviteAsync(Invite invite)
        {
            try
            {
                _context.Add(invite);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AnyInviteAsync(Guid token, string email, int companyId)
        {
            try
            {
                bool result = await _context.Invites
                                            .Where(i => i.CompanyId == companyId)
                                            .AnyAsync(i => i.CompanyToken == token && i.InviteeEmail == email);

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task CancelInviteAsync(int inviteId, int companyId)
        {
            try
            {
                Invite? invite = await _context.Invites
                                               .FirstOrDefaultAsync(i => i.CompanyId == companyId && i.Id == inviteId);

                if(invite is not null)
                {
                    invite.IsValid = false;

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Invite?> GetInviteAsync(int inviteId, int companyId)
        {
            try
            {
                Invite? invite = await _context.Invites
                                               .Include(i => i.Company)
                                               .Include(i => i.Invitor)
                                               .Include(i => i.Invitee)
                                               .Include(i => i.Project)
                                               .FirstOrDefaultAsync(i => i.CompanyId == companyId && i.Id == inviteId);

                return invite;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Invite?> GetInviteAsync(Guid token, string email, int companyId)
        {
            try
            {
                Invite? invite = await _context.Invites
                                               .Include(i => i.Company)
                                               .Include(i => i.Invitor)
                                               .Include(i => i.Invitee)
                                               .Include(i => i.Project)
                                               .FirstOrDefaultAsync(i => i.CompanyId == companyId 
                                                                                     && i.InviteeEmail == email 
                                                                                     && i.CompanyToken == token);

                return invite;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateInviteAsync(Invite invite)
        {
            try
            {
                _context.Update(invite);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> ValidateInviteCodeAsync(Guid? token)
        {
            try
            {
                if(token is null)
                {
                    return false;
                }

                bool result = false;

                Invite? invite = await _context.Invites.FirstOrDefaultAsync(i => i.CompanyToken == token);

                if (invite != null)
                {
                    DateTime inviteDate = invite.InviteDate;

                    bool notExpired = (DateTime.UtcNow - inviteDate).TotalDays <= 7;

                    if (notExpired)
                    {
                        result = invite.IsValid;
                    }
                }

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
