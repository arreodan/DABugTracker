using DABugTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DABugTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<BTUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
            public virtual DbSet<Company> Companies { get; set; }
            public virtual DbSet<Invite> Invites { get; set; }
            public virtual DbSet<Notification> Notifications { get; set; }
            public virtual DbSet<NotificationType> NotificationTypes { get; set; }
            public virtual DbSet<Project> Projects { get; set; }
            public virtual DbSet<ProjectPriority> ProjectPriorities { get; set; }
            public virtual DbSet<Ticket> Tickets { get; set; }
            public virtual DbSet<TicketAttachment> TicketAttachments { get; set; }
            public virtual DbSet<TicketComment> TicketComments { get; set; }
            public virtual DbSet<TicketHistory> TicketHistories { get; set; }
            public virtual DbSet<TicketPriority> TicketPriorities { get; set; }
            public virtual DbSet<TicketStatus> TicketStatuses { get; set; }
            public virtual DbSet<TicketType> TicketTypes { get; set; }

    }
}