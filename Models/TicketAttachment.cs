using DABugTracker.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DABugTracker.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }

        public string? Description { get; set; }


        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        [NotMapped]
        [DisplayName("Select a file")]
        [DataType(DataType.Upload)]
        [MaxFileSize(1024 * 1024)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".pdf" })]
        [Display(Name = "Attatchment Item")]
        public IFormFile? FormFile { get; set; }

        public byte[]? FileData { get; set; }

        public string? FileType { get; set; }

        public string? FileName { get; set; }


        //Foreign Keys
        public int TicketId { get; set; }
        public virtual Ticket? Ticket { get; set; }

        [Required]
        public string? BTUserId { get; set; }
        public virtual BTUser? BTUser { get; set; }

    }
}
