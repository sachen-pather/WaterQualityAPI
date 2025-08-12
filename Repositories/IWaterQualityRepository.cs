using WaterQualityAPI.Models;

namespace WaterQualityAPI.Repositories
{
    public interface IWaterQualityRepository
    {
        Task<IEnumerable<WaterQualityReading>> GetReadingsForBeachAsync(string beachCode);
        Task<WaterQualityReading?> GetLatestReadingForBeachAsync(string beachCode);
        Task<IEnumerable<WaterQualityReading>> GetReadingsForBeachByDateRangeAsync(string beachCode, DateTime startDate, DateTime endDate);
        Task<WaterQualityReading> AddReadingAsync(WaterQualityReading reading);
        Task AddReadingsAsync(IEnumerable<WaterQualityReading> readings);
        Task<WaterQualityReading?> UpdateReadingAsync(WaterQualityReading reading);
        Task<bool> DeleteReadingAsync(int id);
        Task<IEnumerable<WaterQualityReading>> GetAllReadingsAsync();
    }
}