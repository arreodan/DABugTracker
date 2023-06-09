﻿using DABugTracker.Models;

namespace DABugTracker.Services.Interfaces
{
    public interface IBTTicketService
    {
        Task AddTicketAsync(Ticket ticket);
        Task ArchiveTicketAsync(Ticket ticket, int companyId);
        Task<List<Ticket>> GetArchivedTicketsAsync(int companyId);
        Task<List<Ticket>> GetTicketsByCompanyIdAsync(int companyId);
        Task<Ticket> GetTicketByIdAsync(int ticketId, int companyId);
        Task<List<TicketStatus>> GetTicketStatuses();
        Task<List<TicketType>> GetTicketTypes();
        Task<List<TicketPriority>> GetTicketPriorities();
        Task RestoreTicketAsync(Ticket ticket, int companyId);
        Task UpdateTicketAsync(Ticket ticket, int companyId);
        Task<List<Ticket>> GetUnassignedTicketsAsync(int companyId);
        Task<List<Ticket>> GetTicketsByUserIdAsync(string userId);
        Task<Ticket?> GetTicketAsNoTrackingAsync(int ticketId, int companyId);
        Task<List<Ticket>> GetTicketsByProjectAsync(int companyId, int projectId);
        public Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment);
        public Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId);

        Task AddTicketCommentAsync(TicketComment comment);

        Task<List<Ticket>> GetEveryTicketByCompanyIdAsync(int companyId);
        Task<IEnumerable<Ticket>> GetRecentTicketsAsync(int companyId);

    }
}
