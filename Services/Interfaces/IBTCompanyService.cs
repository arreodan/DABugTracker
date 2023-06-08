using DABugTracker.Models;

namespace DABugTracker.Services.Interfaces
{
    public interface IBTCompanyService
    {
        Task<Company?> GetCompanyInfoAsync(int companyId);
        Task<List<BTUser>> GetCompanyMembersAsync(int companyId);
    }
}
