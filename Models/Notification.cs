using System.ComponentModel.DataAnnotations;

namespace DABugTracker.Models
{
    public class Notification
    {
        // Primary Key 
        public int Id { get; set; }
        
        [Required]
        public string? Title { get; set; }
        [Required]
        public string? Message { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        public int NotificationTypeId { get; set; }
        public bool HasBeenViewed { get; set; }


        // Navigation Properties 1 to many 
        public virtual NotificationType? NotificationType { get; set; }

        // Foreign Keys many to 1 
        public int ProjectId { get; set; }
        public virtual Project? Project { get; set; }
        
        public int TicketId { get; set; }

        public virtual Ticket? Ticket { get; set; }
        
        // Foreign Keys 1 to 1 
        public string? SenderId { get; set; }
        public virtual BTUser? Sender { get; set; }
        
        public string? RecipientId { get; set; }
        public virtual BTUser? Recipient { get; set; }
    }
}
