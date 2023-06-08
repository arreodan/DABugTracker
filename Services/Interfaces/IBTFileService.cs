using DABugTracker.Models.Enums;

namespace DABugTracker.Services.Interfaces
{
    public interface IBTFileService
    {
        string ConvertByteArrayToFile(byte[]? fileData, string? extension, DefaultImage defaultImage);

        Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file);
    }
}
