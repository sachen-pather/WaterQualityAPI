using Microsoft.EntityFrameworkCore;
using WaterQualityAPI.Data;
using WaterQualityAPI.Models;

namespace WaterQualityAPI.Repositories
{
    public class BeachRepository : IBeachRepository
    {
        private readonly SupabaseContext _context;

        public BeachRepository(SupabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Beach>> GetAllBeachesAsync()
        {
            return await _context.Beaches
                .Include(b => b.WaterQualityReadings)
                .ToListAsync();
        }

        public async Task<Beach?> GetBeachByCodeAsync(string beachCode)
        {
            return await _context.Beaches
                .Include(b => b.WaterQualityReadings)
                .FirstOrDefaultAsync(b => b.Code == beachCode);
        }

        public async Task<Beach> CreateBeachAsync(Beach beach)
        {
            beach.CreatedAt = DateTime.UtcNow;
            _context.Beaches.Add(beach);
            await _context.SaveChangesAsync();
            return beach;
        }

        public async Task<Beach?> UpdateBeachAsync(Beach beach)
        {
            var existingBeach = await _context.Beaches
                .FirstOrDefaultAsync(b => b.Code == beach.Code);

            if (existingBeach == null)
                return null;

            existingBeach.Name = beach.Name;
            existingBeach.Location = beach.Location;
            existingBeach.Latitude = beach.Latitude;
            existingBeach.Longitude = beach.Longitude;

            await _context.SaveChangesAsync();
            return existingBeach;
        }

        public async Task<bool> DeleteBeachAsync(string beachCode)
        {
            var beach = await _context.Beaches
                .FirstOrDefaultAsync(b => b.Code == beachCode);

            if (beach == null)
                return false;

            _context.Beaches.Remove(beach);
            await _context.SaveChangesAsync();
            return true;
        }

        // Legacy methods for backward compatibility
        public async Task<IEnumerable<WaterQualityReading>> GetAllBeachReadingsAsync()
        {
            return await _context.WaterQualityReadings
                .OrderByDescending(r => r.SamplingDate)
                .ToListAsync();
        }

        public async Task<WaterQualityReading?> GetLatestReadingByCodeAsync(string beachCode)
        {
            return await _context.WaterQualityReadings
                .Where(r => r.BeachCode == beachCode)
                .OrderByDescending(r => r.SamplingDate)
                .FirstOrDefaultAsync();
        }

        public async Task<WaterQualityReading?> UpdateBeachReadingAsync(string beachCode, WaterQualityReading reading)
        {
            var existingReading = await _context.WaterQualityReadings
                .Where(w => w.BeachCode == beachCode)
                .OrderByDescending(w => w.SamplingDate)
                .FirstOrDefaultAsync();

            if (existingReading == null)
                return null;

            // Update properties
            existingReading.EnterococcusCount = reading.EnterococcusCount;
            existingReading.SamplingFrequency = reading.SamplingFrequency;
            existingReading.SamplingDate = reading.SamplingDate;
            existingReading.IsWithinSafetyThreshold = reading.EnterococcusCount <= 100;

            await _context.SaveChangesAsync();
            return existingReading;
        }
    }
}