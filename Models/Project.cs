using Azure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DABugTracker.Models
{
    public class Project
    {
        // Primary Key
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? EndDate { get; set; }

        public int ProjectPriorityId { get; set; }

        public IFormFile? ImageFile { get; set; }
        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }

        public bool Archived { get; set; }

        // Navigation Properties
        public int? CompanyId { get; set; }
        public virtual Company? Company { get; set; }


        public virtual ICollection<ProjectPriority> ProjectPriority { get; set; } = new HashSet<ProjectPriority>();
        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>(); // Unsure about this one
        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
    }
}
