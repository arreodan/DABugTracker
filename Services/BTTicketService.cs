using DABugTracker.Areas.Identity.Pages.Account.Manage;
using DABugTracker.Data;
using DABugTracker.Models;
using DABugTracker.Models.Enums;
using DABugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.EntityFrameworkCore;

namespace DABugTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;

        public BTTicketService(ApplicationDbContext context, IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
        }


        public async Task AddTicketAsync(Ticket ticket)
        {
            try
            {

                TicketStatus? newTicketStatus = await _context.TicketStatuses
                                                                       .FirstOrDefaultAsync(t => t.Name == nameof(BTTicketStatuses.New));


                ticket.TicketStatusId = newTicketStatus!.Id;
                _context.Add(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task ArchiveTicketAsync(Ticket ticket, int companyId)
        {
            try
            {
                if(ticket.Project!.CompanyId == companyId)
                {
                    ticket.Archived = true;
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = await _context.Tickets
                                                     .Where(t => t.Project!.CompanyId == companyId && t.Archived == true)
                                                     .Include(t => t.Project)
                                                     .Include(t => t.DeveloperUser)
                                                     .Include(t => t.SubmitterUser)
                                                     .Include(t => t.TicketPriority)
                                                     .Include(t => t.TicketStatus)
                                                     .Include(t => t.TicketType)
                                                     .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Ticket> GetTicketByIdAsync(int ticketId, int companyId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets
                                               .Where(t => t.Project!.CompanyId == companyId)
                                               .Include(t => t.DeveloperUser)
                                               .Include(t => t.Project)
                                               .Include(t => t.SubmitterUser)
                                               .Include(t => t.TicketPriority)
                                               .Include(t => t.TicketStatus)
                                               .Include(t => t.TicketType)
                                               .Include(t => t.Comments)
                                               .Include(t => t.Attachments)
                                               .Include(t => t.History)
                                               .FirstOrDefaultAsync(m => m.Id == ticketId);

                return ticket!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketPriority>> GetTicketPriorities()
        {
            try
            {
                return await _context.TicketPriorities.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetTicketsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = await _context.Tickets
                                                     .Where(t => t.Project!.CompanyId == companyId && t.Archived == false)
                                                     .Include(t => t.Project)
                                                     .Include(t => t.DeveloperUser)
                                                     .Include(t => t.SubmitterUser)
                                                     .Include(t => t.TicketPriority)
                                                     .Include(t => t.TicketStatus)
                                                     .Include(t => t.TicketType)
                                                     .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetTicketsByUserIdAsync(string userId)
        {
            try
            {
                BTUser? user = await _context.Users.FindAsync(userId);
                if (user is null) return new List<Ticket>();
                // admim -> all tickets for their company 
                if (await _rolesService.IsUserInRole(user, nameof(BTRoles.Admin)))
                {
                    return await GetTicketsByCompanyIdAsync(user.CompanyId);
                }
                // PM -> tickets that belong to their projects
                else if(await _rolesService.IsUserInRole(user, nameof(BTRoles.ProjectManager)))
                {
                    return await _context.Tickets
                                  .Include(t => t.TicketType)
                                  .Include(t => t.TicketStatus)
                                  .Include(t => t.TicketPriority)
                                  .Include(t => t.SubmitterUser)
                                  .Include(t => t.DeveloperUser)
                                  .Include(t => t.Project)
                                    .ThenInclude(p => p!.Members)
                                  .Where(t => !t.Archived && t.Project!.Members.Any(m => m.Id == userId))
                                  .ToListAsync();
                }
                // submitter => tickets theyve submitted 
                // developer -> tickets theyve been assigned or submitted
                else
                {
                    return await _context.Tickets
                                         .Include(t => t.TicketType)
                                         .Include(t => t.TicketStatus)
                                         .Include(t => t.TicketPriority)
                                         .Include(t => t.SubmitterUser)
                                         .Include(t => t.DeveloperUser)
                                         .Include(t => t.Project)
                                           .ThenInclude(p => p!.Members)
                                         .Where(t => !t.Archived && (t.DeveloperUserId == userId || t.SubmitterUserId == userId))
                                         .ToListAsync();

                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketStatus>> GetTicketStatuses()
        {
            try
            {
                return await _context.TicketStatuses.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketType>> GetTicketTypes()
        {
            try
            {
                return await _context.TicketTypes.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RestoreTicketAsync(Ticket ticket, int companyId)
        {
            try
            {
                if (ticket.Project!.CompanyId == companyId)
                {
                    ticket.Archived = false;
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateTicketAsync(Ticket ticket, int companyId)
        {
            try
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Ticket?> GetTicketAsNoTrackingAsync(int ticketId, int companyId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets
                                               .AsNoTracking()
                                               .Where(t => t.Project!.CompanyId == companyId)
                                               .Include(t => t.DeveloperUser)
                                               .Include(t => t.Project)
                                               .Include(t => t.SubmitterUser)
                                               .Include(t => t.TicketPriority)
                                               .Include(t => t.TicketStatus)
                                               .Include(t => t.TicketType)
                                               .FirstOrDefaultAsync(m => m.Id == ticketId);
                return ticket;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetUnassignedTicketsAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = await _context.Tickets
                                     .Where(t => t.Project!.CompanyId == companyId && t.Archived == false && t.DeveloperUserId == null)
                                     .Include(t => t.Project)
                                     .Include(t => t.DeveloperUser)
                                     .Include(t => t.SubmitterUser)
                                     .Include(t => t.TicketPriority)
                                     .Include(t => t.TicketStatus)
                                     .Include(t => t.TicketType)
                                     .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetTicketsByProjectAsync(int companyId, int projectId)
        {
            try
            {
                List<Ticket> tickets = await _context.Tickets
                                                     .Where(t => t.Project!.CompanyId == companyId && t.Archived == false && t.ProjectId == projectId)
                                                     .Include(t => t.Project)
                                                     .Include(t => t.DeveloperUser)
                                                     .Include(t => t.SubmitterUser)
                                                     .Include(t => t.TicketPriority)
                                                     .Include(t => t.TicketStatus)
                                                     .Include(t => t.TicketType)
                                                     .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
        {
            try
            {
                await _context.AddAsync(ticketAttachment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId)
        {
            try
            {
                TicketAttachment? ticketAttachment = await _context.TicketAttachments
                                                                  .Include(t => t.BTUser)
                                                                  .FirstOrDefaultAsync(t => t.Id == ticketAttachmentId);
                return ticketAttachment;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddTicketCommentAsync(TicketComment comment)
        {
            try
            {
                await _context.AddAsync(comment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetEveryTicketByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = await _context.Tickets
                                                     .Where(t => t.Project!.CompanyId == companyId)
                                                     .Include(t => t.Project)
                                                     .Include(t => t.DeveloperUser)
                                                     .Include(t => t.SubmitterUser)
                                                     .Include(t => t.TicketPriority)
                                                     .Include(t => t.TicketStatus)
                                                     .Include(t => t.TicketType)
                                                     .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
