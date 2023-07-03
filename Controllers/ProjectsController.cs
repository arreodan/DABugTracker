using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DABugTracker.Data;
using DABugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using DABugTracker.Models.Enums;
using Microsoft.AspNetCore.Identity;
using DABugTracker.Services.Interfaces;
using DABugTracker.Extensions;
using DABugTracker.Models.ViewModels;
using System.Transactions;
using System.Data;
using System.ComponentModel.Design;

namespace DABugTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _fileService;
        private readonly IBTProjectService _projectService;
        private readonly IBTRolesService _rolesService;

        public ProjectsController(ApplicationDbContext context,
                                  UserManager<BTUser> userManager,
                                  IBTFileService fileService,
                                  IBTProjectService projectService,
                                  IBTRolesService rolesService)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
            _projectService = projectService;
            _rolesService = rolesService;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

            return View(projects);
        }// tested 

        public async Task<IActionResult> UnassignedProjects() // tested
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = await _projectService.GetUnassignedProjectsByCompanyId(companyId);

            return View(projects);
        }

        [HttpGet]
        [Authorize(Roles = nameof(BTRoles.Admin))]
        public async Task<IActionResult> AssignPM(int? id)
        {
            if (id is null or 0)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();
            Project? project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project is null)
            {
                return NotFound();
            }

            List<BTUser> projectManagers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId);
            BTUser? currentPM = await _projectService.GetProjectManagerAsync(id.Value, companyId);

            AssignPMViewModel viewModel = new AssignPMViewModel()
            {
                Project = project,
                PMId = currentPM?.Id,
                PMList = new SelectList(projectManagers, "Id", "FullName", currentPM?.Id)
            };


            return View(viewModel);
        }// tested 
        [HttpPost]
        [Authorize(Roles = nameof(BTRoles.Admin))]
        public async Task<IActionResult> AssignPM(AssignPMViewModel viewModel)// tested 
        {
            if (viewModel.Project?.Id != null)
            {
                if (string.IsNullOrEmpty(viewModel.PMId))
                {
                    await _projectService.RemoveProjectManagerAsync(viewModel.Project.Id, User.Identity!.GetCompanyId());
                }
                else
                {
                    await _projectService.AddProjectManagerAsync(viewModel.PMId, viewModel.Project.Id, User.Identity!.GetCompanyId());
                }

                return RedirectToAction(nameof(Details), new { id = viewModel.Project.Id });
            }

            return BadRequest();
        }

        [HttpGet]
        [Authorize(Roles = nameof(BTRoles.Admin))]
        public async Task<IActionResult> AssignMember(int? id)
        {

            if (id is null or 0)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();
            Project? project = await _projectService.GetProjectByIdAsync(id.Value, companyId);


            if (project is null || project.CompanyId != companyId)
            {
                return NotFound();
            }


            // list of developers already apart of the project 
            List<BTUser> currentDevelopers = await _projectService.GetProjectMembersByRoleAsync(id.Value, nameof(BTRoles.Developer), User.Identity!.GetCompanyId());
            List<BTUser> currentSubmitters = await _projectService.GetProjectMembersByRoleAsync(id.Value, nameof(BTRoles.Submitter), User.Identity!.GetCompanyId());

            // create viewdata for submitters and developers
            ViewData["DeveloperUserId"] = new MultiSelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), User.Identity!.GetCompanyId()), "Id", "FullName", currentDevelopers.Select(d => d.Id));
            ViewData["SubmitterUserId"] = new MultiSelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), User.Identity!.GetCompanyId()), "Id", "FullName", currentSubmitters.Select(s => s.Id));


            return View(project);
        }

        [HttpPost]
        [Authorize(Roles = nameof(BTRoles.Admin))]
        public async Task<IActionResult> AssignMember(int? id,[Bind("Id")] Project project, IEnumerable<string> SelectedDev, IEnumerable<string> SelectedSub)     // add parameters to capture the selected devs and subs
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (SelectedDev != null || SelectedSub != null)
            {
            // - remove everyone who is a developer or a submitter (condier in your loop whether the member being removed is NOT a projectManager)
                await _projectService.RemoveMembersFromProjectAsync(id.Value, User.Identity!.GetCompanyId()); // removes both devs and subs

            //1. combine the dev ids and sub ids into one list




            //2. pass the combined list to a service method (AddMembersToProjectAsync) that adds members to project (along with the project id)
            // - the AddMemberToProjectAsync() method can be used to help achieve this
                await _projectService.AddMembersToProjectAsync(id.Value, User.Identity!.GetCompanyId(), SelectedDev); // adds devs

                await _projectService.AddMembersToProjectAsync(id.Value, User.Identity!.GetCompanyId(), SelectedSub); // add subs
            }


            // - add the members selected by the end user


            if (project is null)
            {
                return NotFound();
            }

            // create viewdata for submitters and developers
            List<BTUser> currentDevelopers = await _projectService.GetProjectMembersByRoleAsync(id.Value, nameof(BTRoles.Developer), User.Identity!.GetCompanyId());
            List<BTUser> currentSubmitters = await _projectService.GetProjectMembersByRoleAsync(id.Value, nameof(BTRoles.Submitter), User.Identity!.GetCompanyId());
            ViewData["DeveloperUserId"] = new MultiSelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), User.Identity!.GetCompanyId()), "Id", "FullName", currentDevelopers.Select(d => d.Id));
            ViewData["SubmitterUserId"] = new MultiSelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), User.Identity!.GetCompanyId()), "Id", "FullName", currentSubmitters.Select(s => s.Id));


            return RedirectToAction(nameof(Details), new { id });
        }




        public async Task<IActionResult> ArchivedProjects()// tested 
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Project>? projects = await _projectService.GetArchivedProjectsByCompanyIdAsync(companyId);

            return View(projects);
        }
        public async Task<IActionResult> MyProjects() // tested
        {
            BTUser? user = await _userManager.GetUserAsync(User);

            List<Project> projects = await _projectService.GetAllUserProjectsAsync(user!.Id);

            return View(projects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id) //tested
        {
            if (id == null)
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectByIdAsync(id.Value, User.Identity!.GetCompanyId());

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Create() //tested
        {
            Project project = new Project();

            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetProjectPrioritiesAsync(), "Id", "Name", project.ProjectPriorityId);
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Create([Bind("Name,Description,StartDate,EndDate,ProjectPriorityId,ImageFormFile")] Project project) //tested
        {
            ModelState.Remove(nameof(project.CompanyId));

            if (ModelState.IsValid)
            {
                project.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                project.StartDate = DateTime.SpecifyKind(project.StartDate, DateTimeKind.Utc);
                project.EndDate = DateTime.SpecifyKind(project.EndDate, DateTimeKind.Utc);

                BTUser? user = await _userManager.GetUserAsync(User);
                project.CompanyId = user!.CompanyId;

                if (project.ImageFormFile is not null)
                {
                    project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                    project.ImageFileType = project.ImageFormFile.ContentType;
                }

                if (User.IsInRole(nameof(BTRoles.ProjectManager)))
                {
                    project.Members.Add(user);
                }

                await _projectService.AddProjectAsync(project);
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetProjectPrioritiesAsync(), "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Edit/5

        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Edit(int? id) // tested
        {
            if (id == null)
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectByIdAsync(id.Value, User.Identity!.GetCompanyId());

            if (project == null)
            {
                return NotFound();
            }
            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetProjectPrioritiesAsync(), "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageFileData,ImageFileType,Archived,CompanyId,ImageFormFile")] Project project) // tested
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    project.Created = DateTime.SpecifyKind(project.Created, DateTimeKind.Utc);
                    project.StartDate = DateTime.SpecifyKind(project.StartDate, DateTimeKind.Utc);
                    project.EndDate = DateTime.SpecifyKind(project.EndDate, DateTimeKind.Utc);

                    if (project.ImageFormFile != null)
                    {
                        project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                        project.ImageFileType = project.ImageFormFile.ContentType;
                    }


                    await _projectService.UpdateProjectAsync(project, User.Identity!.GetCompanyId());
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetProjectPrioritiesAsync(), "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Archive(int? id)// tested
         {
            if (id == null)
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectByIdAsync(id.Value, User.Identity!.GetCompanyId());


            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Archive(int id) // tested
        {
            Project? project = await _projectService.GetProjectByIdAsync(id, User.Identity!.GetCompanyId());

            if (project != null)
            {
                await _projectService.ArchiveProjectAsync(project, User.Identity!.GetCompanyId());
            }
            return RedirectToAction("Details", new { project!.Id });
        }
        
        // POST: Projects/Delete/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Restore(int id) // tested
        {
            Project? project = await _projectService.GetProjectByIdAsync(id, User.Identity!.GetCompanyId());

            if (project != null)
            {
                await _projectService.RestoreProjectAsync(project, User.Identity!.GetCompanyId());
            }
            return RedirectToAction("Details", new { project!.Id });
        }



    }
}
