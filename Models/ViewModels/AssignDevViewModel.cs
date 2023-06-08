﻿using Microsoft.AspNetCore.Mvc.Rendering;

namespace DABugTracker.Models.ViewModels
{
    public class AssignDevViewModel
    {
        public Ticket? Ticket { get; set; }
        public SelectList? DevList { get; set; }
        public string? DevId { get; set; }
    }
}
