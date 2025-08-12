using WaterQualityAPI.Models;

namespace WaterQualityAPI.Repositories
{
    public interface IBeachRepository
    {
        Task<IEnumerable<Beach>> GetAllBeachesAsync();
        Task<Beach?> GetBeachByCodeAsync(string beachCode);
        Task<Beach> CreateBeachAsync(Beach beach);
        Task<Beach?> UpdateBeachAsync(Beach beach);
        Task<bool> DeleteBeachAsync(string beachCode);
        Task<IEnumerable<WaterQualityReading>> GetAllBeachReadingsAsync();
        Task<WaterQualityReading?> GetLatestReadingByCodeAsync(string beachCode);
        Task<WaterQualityReading?> UpdateBeachReadingAsync(string beachCode, WaterQualityReading reading);
    }
}