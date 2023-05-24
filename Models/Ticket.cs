using System.ComponentModel.DataAnnotations;

namespace DABugTracker.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Ticket Title")]
        public string? Title { get; set; }
        [Required]
        public string? Description { get; set; }


        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }

        public bool Archived { get; set; }
        public bool ArchivedByProject { get; set; }


        // Navigation Properties 

        //Foreign Keys
        public int ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        public int TicketTypeId { get; set; }
        public virtual TicketType? TicketType { get; set; }

        public int TicketStatusId { get; set; }
        public virtual TicketStatus? TicketStatus { get; set; }

        public int TicketPriorityId { get; set; }
        public virtual TicketPriority? TicketPriority { get; set; }
        
        public string? DeveloperUserId { get; set; }
        public virtual BTUser? DeveloperUser { get; set; }

        public string? SubmitterUserId { get; set; }
        public virtual BTUser? SubmitterUser { get; set; }


        public virtual ICollection<TicketComment> Comments { get; set; } = new HashSet<TicketComment>();
        public virtual ICollection<TicketAttachment> Attachments { get; set; } = new HashSet<TicketAttachment>();
        public virtual ICollection<TicketHistory> History { get; set; } = new HashSet<TicketHistory>();



    }
}
