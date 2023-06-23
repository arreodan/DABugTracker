using DABugTracker.Data;
using DABugTracker.Extensions;
using DABugTracker.Models;
using DABugTracker.Models.ChartModels;
using DABugTracker.Models.Enums;
using DABugTracker.Models.ViewModels;
using DABugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DABugTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private readonly ApplicationDbContext _context;
		private readonly UserManager<BTUser> _userManager;
		private readonly IBTFileService _fileService;
		private readonly IBTProjectService _projectService;
		private readonly IBTRolesService _rolesService;
		private readonly IBTTicketService _ticketService;
		private readonly IBTCompanyService _companyService;

		public HomeController(ILogger<HomeController> logger,
									  ApplicationDbContext context,
									  UserManager<BTUser> userManager,
									  IBTFileService fileService,
									  IBTProjectService projectService,
									  IBTRolesService rolesService,
									  IBTTicketService ticketService,
									  IBTCompanyService companyService)
		{
			_logger = logger;
			_context = context;
			_userManager = userManager;
			_fileService = fileService;
			_projectService = projectService;
			_rolesService = rolesService;
			_ticketService = ticketService;
			_companyService = companyService;
		}

		public IActionResult Index()
        {
            return View();
        }
		public async Task<IActionResult> Dashboard()
		{
			int companyId = User.Identity!.GetCompanyId();

			DashboardViewModel viewModel = new DashboardViewModel()
			{
				Projects = await _projectService.GetEveryProjectByCompanyIdAsync(companyId),
				Tickets = await _ticketService.GetEveryTicketByCompanyIdAsync(companyId),
				Members = await _companyService.GetCompanyMembersAsync(companyId),
				Company = await _companyService.GetCompanyInfoAsync(companyId)
			};

			return View(viewModel);
		}

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpPost]
        public async Task<JsonResult> PlotlyBarChart()
        {
            PlotlyBarData plotlyData = new();
            List<PlotlyBar> barData = new();

            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

            //Bar One
            PlotlyBar barOne = new()
            {
                X = projects.Select(p => p.Name).ToArray(),
                Y = projects.SelectMany(p => p.Tickets).GroupBy(t => t.ProjectId).Select(g => g.Count()).ToArray(),
                Name = "Tickets",
                Type = "bar"
            };

            //Bar Two
            PlotlyBar barTwo = new()
            {
                X = projects.Select(p => p.Name).ToArray(),
                Y = projects.Select(async p => (await _projectService.GetProjectMembersByRoleAsync(p.Id, nameof(BTRoles.Developer), companyId)).Count).Select(c => c.Result).ToArray(),
                Name = "Developers",
                Type = "bar"
            };

            barData.Add(barOne);
            barData.Add(barTwo);

            plotlyData.Data = barData;

            return Json(plotlyData);
        }
    }
}