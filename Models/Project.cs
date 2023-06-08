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
        [Display(Name = "Project Name")]
        public string? Name { get; set; }

        [Required]
        [Display(Name = "Project Description")]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Project Priority")]
        public int ProjectPriorityId { get; set; }

        [NotMapped]
        [Display(Name = "Project Image")]
        public IFormFile? ImageFormFile { get; set; }
        public byte[]? ImageFileData { get; set; }
        public string? ImageFileType { get; set; }

        public bool Archived { get; set; }

        // Navigation Properties
        public int CompanyId { get; set; }
        public virtual Company? Company { get; set; }


        public virtual ProjectPriority? ProjectPriority { get; set; }
        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>(); 
        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
    }
}
