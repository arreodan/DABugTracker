using System.ComponentModel.DataAnnotations;

namespace DABugTracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Property Name")]
        public string? PropertyName { get; set; }

        [Required]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        public string OldValue { get; set; }
        public string NewValue { get; set; }


        //Foreign Keys
        public int TicketId { get; set; }
        public virtual Ticket? Ticket { get; set; }

        [Required]
        public string? UserId { get; set; }
        public virtual BTUser? User { get; set; }
    }
}
