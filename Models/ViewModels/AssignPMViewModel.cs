using Microsoft.AspNetCore.Mvc.Rendering;
using System.Composition.Convention;

namespace DABugTracker.Models.ViewModels
{
    public class AssignPMViewModel
    {
        public Project? Project { get; set; }
        public SelectList? PMList { get; set; }
        public string? PMId { get; set; }
    }
}
