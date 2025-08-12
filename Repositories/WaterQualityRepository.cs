using Microsoft.EntityFrameworkCore;
using WaterQualityAPI.Data;
using WaterQualityAPI.Models;

namespace WaterQualityAPI.Repositories
{
    public class WaterQualityRepository : IWaterQualityRepository
    {
        private readonly SupabaseContext _context;

        public WaterQualityRepository(SupabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WaterQualityReading>> GetReadingsForBeachAsync(string beachCode)
        {
            return await _context.WaterQualityReadings
                .Where(r => r.BeachCode == beachCode)
                .OrderByDescending(r => r.SamplingDate)
                .ToListAsync();
        }

        public async Task<WaterQualityReading?> GetLatestReadingForBeachAsync(string beachCode)
        {
            return await _context.WaterQualityReadings
                .Where(r => r.BeachCode == beachCode)
                .OrderByDescending(r => r.SamplingDate)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<WaterQualityReading>> GetReadingsForBeachByDateRangeAsync(string beachCode, DateTime startDate, DateTime endDate)
        {
            return await _context.WaterQualityReadings
                .Where(r => r.BeachCode == beachCode &&
                           r.SamplingDate >= startDate &&
                           r.SamplingDate <= endDate)
                .OrderByDescending(r => r.SamplingDate)
                .ToListAsync();
        }

        public async Task<WaterQualityReading> AddReadingAsync(WaterQualityReading reading)
        {
            // Calculate safety threshold (assuming 100 is the safe limit)
            reading.IsWithinSafetyThreshold = reading.EnterococcusCount <= 100;

            // Ensure the beach exists before adding the reading
            await EnsureBeachExistsAsync(reading.BeachCode);

            _context.WaterQualityReadings.Add(reading);
            await _context.SaveChangesAsync();
            return reading;
        }

        public async Task AddReadingsAsync(IEnumerable<WaterQualityReading> readings)
        {
            var readingsList = readings.ToList();

            if (!readingsList.Any())
                return;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Step 1: Get all unique beach codes from the readings
                var beachCodes = readingsList
                    .Select(r => r.BeachCode)
                    .Distinct()
                    .ToList();

                // Step 2: Find which beach codes already exist in the database
                var existingBeachCodes = await _context.Beaches
                    .Where(b => beachCodes.Contains(b.Code))
                    .Select(b => b.Code)
                    .ToListAsync();

                // Step 3: Identify new beach codes that need to be created
                var newBeachCodes = beachCodes
                    .Except(existingBeachCodes)
                    .ToList();

                // Step 4: Create new Beach entities for missing beach codes
                if (newBeachCodes.Any())
                {
                    var newBeaches = newBeachCodes.Select(code => new Beach
                    {
                        Code = code,
                        Name = GenerateBeachNameFromCode(code),
                        Location = "Auto-generated from PDF upload", // Default location
                        Latitude = 0.0, // Default coordinates - you may want to set these properly
                        Longitude = 0.0,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    await _context.Beaches.AddRangeAsync(newBeaches);
                    await _context.SaveChangesAsync();
                }

                // Step 5: Process the water quality readings
                foreach (var reading in readingsList)
                {
                    // Calculate safety threshold for each reading
                    reading.IsWithinSafetyThreshold = reading.EnterococcusCount <= 100;
                }

                // Step 6: Add all water quality readings
                await _context.WaterQualityReadings.AddRangeAsync(readingsList);
                await _context.SaveChangesAsync();

                // Step 7: Commit the transaction
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                // Rollback the transaction if anything fails
                await transaction.RollbackAsync();
                throw; // Re-throw the exception
            }
        }

        public async Task<WaterQualityReading?> UpdateReadingAsync(WaterQualityReading reading)
        {
            var existingReading = await _context.WaterQualityReadings
                .FirstOrDefaultAsync(r => r.Id == reading.Id);

            if (existingReading == null)
                return null;

            existingReading.EnterococcusCount = reading.EnterococcusCount;
            existingReading.SamplingDate = reading.SamplingDate;
            existingReading.SamplingFrequency = reading.SamplingFrequency;
            existingReading.IsWithinSafetyThreshold = reading.EnterococcusCount <= 100;

            await _context.SaveChangesAsync();
            return existingReading;
        }

        public async Task<bool> DeleteReadingAsync(int id)
        {
            var reading = await _context.WaterQualityReadings.FindAsync(id);
            if (reading == null)
                return false;

            _context.WaterQualityReadings.Remove(reading);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<WaterQualityReading>> GetAllReadingsAsync()
        {
            return await _context.WaterQualityReadings
                .OrderByDescending(r => r.SamplingDate)
                .ToListAsync();
        }

        // Helper method to ensure a single beach exists
        private async Task EnsureBeachExistsAsync(string beachCode)
        {
            var beachExists = await _context.Beaches
                .AnyAsync(b => b.Code == beachCode);

            if (!beachExists)
            {
                var newBeach = new Beach
                {
                    Code = beachCode,
                    Name = GenerateBeachNameFromCode(beachCode),
                    Location = "Auto-generated from PDF upload",
                    Latitude = 0.0,
                    Longitude = 0.0,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Beaches.Add(newBeach);
                await _context.SaveChangesAsync();
            }
        }

        // Helper method to generate a beach name from the code
        private static string GenerateBeachNameFromCode(string beachCode)
        {
            // You can customize this logic based on your beach code naming convention
            // For now, it just formats the code nicely
            if (string.IsNullOrEmpty(beachCode))
                return "Unknown Beach";

            // Example: "XCN04" -> "Beach XCN04"
            // You might want to implement more sophisticated naming logic here
            return $"Beach {beachCode.ToUpperInvariant()}";
        }
    }
}