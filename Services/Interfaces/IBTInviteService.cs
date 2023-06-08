using DABugTracker.Models;

namespace DABugTracker.Services.Interfaces
{
    public interface IBTInviteService
    {
        Task<bool> AcceptInviteAsync(Guid? token, string userId, int companyId);
        Task AddnewInviteAsync(Invite invite);
        Task<bool> AnyInviteAsync(Guid token, string email, int companyId);
        Task<Invite?> GetInviteAsync(int inviteId, int companyId);
        Task<Invite?> GetInviteAsync(Guid token, string email, int companyId);
        Task<bool> ValidateInviteCodeAsync(Guid? token);
        Task CancelInviteAsync(int inviteId, int companyId);
        Task UpdateInviteAsync(Invite invite);

        
    }
}
