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
        public IFormFile? FormFile { get; set; }

        public byte[]? FileData { get; set; }

        public string? FileType { get; set; }


        //Foreign Keys
        public int TicketId { get; set; }
        public virtual Ticket? Ticket { get; set; }

        public int BTUserId { get; set; }
        public virtual BTUser? BTUser { get; set; }

    }
}
