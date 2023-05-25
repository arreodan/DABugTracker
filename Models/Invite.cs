using System.ComponentModel.DataAnnotations;

namespace DABugTracker.Models
{
    public class Invite
    {
        // Primary Key
        public int Id { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime InviteDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? JoinDate { get; set; }

        public Guid CompanyToken { get; set; }

        [Required]
        [Display(Name = "Invitee Email")]
        public string? InviteeEmail { get; set; }

        [Required]
        [Display(Name = "Invitee First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? InviteeFirstName { get; set; }

        [Required]
        [Display(Name = "Invitee Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? InviteeLastName { get; set; }

        public string? Message { get; set; }

        public bool IsValid { get; set; }

        // Navigation Properties

        // Foreign Keys many to 1 
        public int CompanyId { get; set; }
        public virtual Company? Company { get; set; }

        public int? ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        // Foreign Keys 1 to 1 
        [Required]
        public string? InvitorId { get; set; }
        public virtual BTUser? Invitor { get; set; }

        public string? InviteeId { get; set; }
        public virtual BTUser? Invitee { get; set; }
    }
}
