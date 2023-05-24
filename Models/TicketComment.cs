using System.ComponentModel.DataAnnotations;

namespace DABugTracker.Models
{
    public class TicketComment
    {
        public int Id { get; set; }

        [Required]
        public string? Comment { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        // Foreign Keys 

        public int TicketId { get; set; }
        public virtual Ticket? Ticket { get; set; }

        public string? UserId { get; set; }
        public virtual BTUser? User { get; set; }
    }
}
