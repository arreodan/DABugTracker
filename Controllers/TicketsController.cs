using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DABugTracker.Data;
using DABugTracker.Models;
using Microsoft.AspNetCore.Identity;
using DABugTracker.Models.Enums;
using System.Net.Sockets;
using DABugTracker.Services.Interfaces;
using DABugTracker.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.CodeAnalysis.Operations;
using DABugTracker.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.ComponentModel.Design;
using DABugTracker.Services;
using System.IO;
using Org.BouncyCastle.Bcpg;

namespace DABugTracker.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _ticketService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTProjectService _projectService;
        private readonly IBTTicketHistoryService _ticketHistory;
        private readonly IBTFileService _fileService;

        public TicketsController(ApplicationDbContext context,
                                 UserManager<BTUser> userManager,
                                 IBTTicketService ticketService,
                                 IBTRolesService rolesService,
                                 IBTProjectService projectService,
                                 IBTTicketHistoryService ticketHistory,
                                 IBTFileService fileService)
        {
            _context = context;
            _userManager = userManager;
            _ticketService = ticketService;
            _rolesService = rolesService;
            _projectService = projectService;
            _ticketHistory = ticketHistory;
            _fileService = fileService;
        }

        // GET: Tickets
        public async Task<IActionResult> AllTickets() // tested
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Ticket> tickets = await _ticketService.GetTicketsByCompanyIdAsync(companyId);

            return View(tickets);
        }
        public async Task<IActionResult> ArchivedTickets() // tested
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Ticket> tickets = await _ticketService.GetArchivedTicketsAsync(companyId);

            return View(tickets);
        }

        public async Task<IActionResult> MyTickets()
        {
            BTUser? user = await _userManager.GetUserAsync(User);

            List<Ticket> tickets = await _ticketService.GetTicketsByUserIdAsync(user!.Id);

            return View(tickets);
        }
        public async Task<IActionResult> UnassignedTickets()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Ticket> tickets = await _ticketService.GetUnassignedTicketsAsync(companyId);

            return View(tickets);
        }

        [HttpGet]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> AssignDev(int? id)
        {
            if (id is null or 0)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();
            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id.Value, companyId);

            if (ticket is null)
            {
                return NotFound();
            }

            List<BTUser> developers = await _projectService.GetProjectMembersByRoleAsync(ticket.ProjectId, nameof(BTRoles.Developer), companyId);
            BTUser? currentDev = ticket.DeveloperUser;

            AssignDevViewModel viewModel = new AssignDevViewModel()
            {
                Ticket = ticket,
                DevId = currentDev?.Id,
                DevList = new SelectList(developers, "Id", "FullName", currentDev?.Id)
            };


            return View(viewModel);
        }// tested 
        [HttpPost]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> AssignDev(AssignDevViewModel viewModel)
        {
            if (viewModel.Ticket?.Id != null)
            {
                Ticket? ticket = await _ticketService.GetTicketByIdAsync(viewModel.Ticket.Id, User.Identity!.GetCompanyId());
                if (ticket != null)
                {
                    ticket.DeveloperUserId = viewModel.DevId;
                    await _ticketService.UpdateTicketAsync(ticket, User.Identity!.GetCompanyId());
                    return RedirectToAction(nameof(Details), new { id = viewModel.Ticket.Id });
                }


            }

            return BadRequest();
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id) // tested
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id.Value, companyId);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create() // tested
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name");
            ViewData["TicketPriorityId"] = new SelectList(await _ticketService.GetTicketPriorities(), "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(await _ticketService.GetTicketStatuses(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _ticketService.GetTicketTypes(), "Id", "Name");

            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,ProjectId,TicketTypeId,TicketPriorityId")] Ticket ticket) // tested
        {
            ModelState.Remove("SubmitterUserId"); // needed when item is required for database ([Required] in model) but not accepting it from the form. Tells modelstate to ignore it and I will deal with it
            string? userId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                ticket.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                ticket.SubmitterUserId = userId;

                await _ticketService.AddTicketAsync(ticket);
                await _ticketHistory.AddHistoryAsync(null!, ticket, userId!);

                return RedirectToAction(nameof(AllTickets));
            }

            ViewData["ProjectId"] = new SelectList(await _ticketService.GetTicketsByCompanyIdAsync(User.Identity!.GetCompanyId()), "Id", "Name", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(await _ticketService.GetTicketPriorities(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _ticketService.GetTicketStatuses(), ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _ticketService.GetTicketTypes(), "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;
            ModelState.Remove("BTUserId");

            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                ticketAttachment.FileType = ticketAttachment.FormFile.ContentType;

                ticketAttachment.Created = DateTime.UtcNow;
                ticketAttachment.BTUserId = _userManager.GetUserId(User);

                await _ticketService.AddTicketAttachmentAsync(ticketAttachment);
                statusMessage = "Success: New attachment added to Ticket.";

                await _ticketHistory.AddHistoryAsync(ticketAttachment.TicketId, nameof(TicketAttachment), ticketAttachment.BTUserId!);
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id) // tested
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id.Value, User.Identity!.GetCompanyId());

            if (ticket == null)
            {
                return NotFound();
            }

            ViewData["TicketPriorityId"] = new SelectList(await _ticketService.GetTicketPriorities(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _ticketService.GetTicketStatuses(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _ticketService.GetTicketTypes(), "Id", "Name", ticket.TicketTypeId);
            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Name", ticket.DeveloperUserId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,SubmitterUserId")] Ticket ticket) // tested
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                try
                {
                    ticket.Created = DateTime.SpecifyKind(ticket.Created, DateTimeKind.Utc);
                    ticket.Updated = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

                    Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, User.Identity!.GetCompanyId());

                    await _ticketService.UpdateTicketAsync(ticket, User.Identity!.GetCompanyId());
                    ticket = (await _ticketService.GetTicketByIdAsync(ticket.Id, User.Identity!.GetCompanyId()))!;


                    await _ticketHistory.AddHistoryAsync(oldTicket!, ticket, _userManager.GetUserId(User)!);
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction("Details", new { ticket!.Id });
            }

            ViewData["TicketPriorityId"] = new SelectList(await _ticketService.GetTicketPriorities(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _ticketService.GetTicketStatuses(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _ticketService.GetTicketTypes(), "Id", "Name", ticket.TicketTypeId);
            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Name", ticket.DeveloperUserId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Archive(int? id) // tested
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id.Value, User.Identity!.GetCompanyId());

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id) // tested
        {
            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id, User.Identity!.GetCompanyId());
            if (ticket != null)
            {
                await _ticketService.ArchiveTicketAsync(ticket, User.Identity!.GetCompanyId());
            }
            return RedirectToAction("Details", new { ticket!.Id });
        }

        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            string fileName = ticketAttachment.FileName!;
            byte[] fileData = ticketAttachment.FileData!;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTicketComment([Bind("Id, Comment, TicketId")] TicketComment ticketComment)
        {
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                ticketComment.Created = DateTime.UtcNow;
                ticketComment.UserId = _userManager.GetUserId(User);

                await _ticketService.AddTicketCommentAsync(ticketComment);

                await _ticketHistory.AddHistoryAsync(ticketComment.TicketId, nameof(TicketComment), ticketComment.UserId!);
            }
            
                return RedirectToAction("Details", new { id = ticketComment.TicketId });

        }

    }
}
