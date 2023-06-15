using DABugTracker.Data;
using DABugTracker.Models;
using DABugTracker.Models.Enums;
using DABugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace DABugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;

        public BTProjectService(ApplicationDbContext context, IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
        }

        public async Task AddProjectAsync(Project project)
        {
            try
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        } // tested

        public async Task ArchiveProjectAsync(Project project, int companyId)
        {
            try
            {
                if (project.CompanyId == companyId)
                {
                    project.Archived = true;

                    // archive all the tickets in project

                    foreach (Ticket ticket in project.Tickets)
                    {
                        // archive by projcet if ticket is not already archived
                        if (ticket.Archived == false) ticket.ArchivedByProject = true;


                        ticket.Archived = true; // double check this line

                    }
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Project> project = await _context.Projects
                                      .Where(p => p.CompanyId == companyId && p.Archived == false)
                                      .Include(p => p.Tickets)
                                      .Include(p => p.ProjectPriority)
                                      .Include(p => p.Members)
                                      .ToListAsync();

                return project;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priority)
        {
            try
            {
                List<Project> project = await _context.Projects
                                                      .Where(p => p.CompanyId == companyId && p.Archived == false)
                                                      .Include(p => p.Tickets)
                                                      .Include(p => p.ProjectPriority)
                                                      .Include(p => p.Members)
                                                      .Where(p => string.Equals(priority, p.ProjectPriority!.Name))
                                                      .ToListAsync();

                return project;
            }
            catch (Exception)
            {
                return new List<Project>();
                throw;
            }
        }

        public async Task<List<Project>> GetAllUserProjectsAsync(string userId)
        {
            try
            {
                BTUser? user = await _context.Users.FindAsync(userId);

                if (await _rolesService.IsUserInRole(user, nameof(BTRoles.Admin)))
                {
                    List<Project> project = await _context.Projects
                      .Where(p => p.CompanyId == user.CompanyId)
                      .Include(p => p.Tickets)
                      .Include(p => p.ProjectPriority)
                      .Include(p => p.Members)
                      .ToListAsync();

                    return project;
                }
                else
                {

                    List<Project> projects = await _context.Projects
                                         .Include(p => p.Members)
                                         .Where(p => p.Members.Any(m => m.Id == userId))
                                         .Include(p => p.ProjectPriority)
                                         .Include(p => p.Tickets)
                                         .ToListAsync();

                    return projects;
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Project> project = await _context.Projects
                                                      .Where(p => p.CompanyId == companyId && p.Archived == true)
                                                      .Include(p => p.Tickets)
                                                      .Include(p => p.ProjectPriority)
                                                      .Include(p => p.Members)
                                                      .ToListAsync();

                return project;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Project?> GetProjectByIdAsync(int projectId, int companyId)
        {
            try
            {
                return await _context.Projects
                                     .Include(p => p.Company)
                                     .Include(p => p.ProjectPriority)
                                     .Include(p => p.Members)
                                     .Include(p => p.Tickets)
                                        .ThenInclude(t => t.SubmitterUser)
                                     .Include(p => p.Tickets)
                                        .ThenInclude(t => t.DeveloperUser)
                                     .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            try
            {
                return await _context.ProjectPriorities.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RestoreProjectAsync(Project project, int companyId)
        {
            try
            {
                if (project.CompanyId == companyId)
                {
                    project.Archived = false;

                    foreach (Ticket ticket in project.Tickets)
                    {
                        if (ticket.ArchivedByProject == true) ticket.Archived = false;

                        ticket.ArchivedByProject = false;
                    }

                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateProjectAsync(Project project, int companyId)
        {
            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<BTUser?> GetProjectManagerAsync(int projectId, int companyId)
        {
            try
            {
                Project? project = await _context.Projects
                                                 .AsNoTracking()
                                                 .Include(p => p.Members)
                                                 .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                if (project is not null)
                {
                    foreach (BTUser member in project.Members)
                    {
                        if (await _rolesService.IsUserInRole(member, nameof(BTRoles.ProjectManager)))
                        {
                            return member;
                        }
                    }
                }

                return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId, int companyId)
        {
            try
            {
                // get the projet for this company
                Project? project = await _context.Projects
                                                 .Include(p => p.Members)
                                                 .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                // get the user for this company
                BTUser? projectManager = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.CompanyId == companyId);

                if (project is not null && projectManager is not null)
                {
                    // make sure user is PM
                    if (!await _rolesService.IsUserInRole(projectManager, nameof(BTRoles.ProjectManager))) return false;
                    // remove any potentialy eisting PM
                    await RemoveProjectManagerAsync(projectId, companyId);
                    // assign PM
                    project.Members.Add(projectManager);
                    // save change
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int projectId, int companyId)
        {
            try
            {
                Project? project = await _context.Projects
                                                 .Include(p => p.Members)
                                                 .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                if (project is not null)
                {
                    foreach (BTUser member in project.Members)
                    {
                        if (await _rolesService.IsUserInRole(member, nameof(BTRoles.ProjectManager)))
                        {
                            project.Members.Remove(member);
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Project>> GetUnassignedProjectsByCompanyId(int companyId) // todo: Add to Projects controller
        {
            try
            {
                List<Project> allProjects = await _context.Projects
                                                          .Where(p => p.CompanyId == companyId)
                                                          .Include(p => p.Tickets)
                                                          .Include(p => p.ProjectPriority)
                                                          .Include(p => p.Members)
                                                          .ToListAsync();

                List<Project> unassignedProjects = new();

                foreach (Project project in allProjects)
                {
                    BTUser? projectManageer = await GetProjectManagerAsync(project.Id, companyId);

                    if (projectManageer is null) unassignedProjects.Add(project);
                }
                return unassignedProjects;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string roleName, int companyId)
        {
            try
            {
                Project? projectById = await GetProjectByIdAsync(projectId, companyId);

                List<BTUser> membersInRole = new();

                if (projectById is not null)
                {
                    foreach (BTUser member in projectById.Members)
                    {
                        if (await _rolesService.IsUserInRole(member, roleName))
                        {
                            membersInRole.Add(member);
                        }
                    }
                }
                return membersInRole;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AddMemberToProjectAsync(BTUser member, int projectId, int companyId)
        {
            try
            {
                Project? project = await _context.Projects
                                 .Include(p => p.Members)
                                 .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);


                if (project is not null && member.CompanyId == project.CompanyId)
                {
                    project.Members.Add(member);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<bool> RemoveMemberFromProjectAsync(BTUser member, int projectId, int companyId)
        {
            throw new NotImplementedException();
        }


        public async Task RemoveMembersFromProjectAsync(int projectId, int companyId)
        {
            try
            {
                Project? project = await _context.Projects
                                                 .Include(p => p.Members)
                                                 .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                if (project is not null)
                {
                    foreach (BTUser member in project.Members)
                    {
                        if (await _rolesService.IsUserInRole(member, nameof(BTRoles.Developer)) || (await _rolesService.IsUserInRole(member, nameof(BTRoles.Submitter))))
                        {
                            project.Members.Remove(member);
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task AddMembersToProjectAsync(int projectId, int companyId, IEnumerable<string> selectedIds)
        {
            try
            {
                Project? project = await _context.Projects
                                 .Include(p => p.Members)
                                 .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                if (project is not null)
                {
                    foreach (string id in selectedIds)
                    {
                        BTUser? selectedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == companyId);

                        project.Members.Add(selectedUser);
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
