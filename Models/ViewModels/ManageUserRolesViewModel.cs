using Microsoft.AspNetCore.Mvc.Rendering;

namespace DABugTracker.Models.ViewModels
{
    public class ManageUserRolesViewModel
    {
        public SelectList? Roles { get; set; }
        public BTUser? User { get; set; }
        public string? SelectedRole { get; set; }
    }
}
