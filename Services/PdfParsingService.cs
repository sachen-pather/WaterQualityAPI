using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text.RegularExpressions;
using WaterQualityAPI.Models;
namespace WaterQualityAPI.Services;

public class PdfParsingService
{
    public async Task<List<WaterQualityReading>> ParsePdfAsync(Stream pdfStream)
    {
        var readings = new List<WaterQualityReading>();

        // Use Task.Run since PDF parsing is CPU-intensive
        await Task.Run(() =>
        {
            try
            {
                using var pdfReader = new PdfReader(pdfStream);
                using var pdfDocument = new PdfDocument(pdfReader);

                for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                {
                    var page = pdfDocument.GetPage(i);
                    var strategy = new LocationTextExtractionStrategy();
                    var content = PdfTextExtractor.GetTextFromPage(page, strategy);

                    // Process the page content
                    ParsePageContent(content, readings);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error parsing PDF: {ex.Message}");
                throw; // Rethrow to be handled by the controller
            }
        });

        return readings;
    }

    private void ParsePageContent(string content, List<WaterQualityReading> readings)
    {
        // Split content into lines
        var lines = content.Split('\n');

        // First pass: Try to match with the standard pattern
        foreach (var line in lines)
        {
            // Skip empty lines and header lines
            if (string.IsNullOrWhiteSpace(line) ||
                line.Contains("Site Order") ||
                line.Contains("Grade Enterococci") ||
                line.Contains("RECREATIONAL"))
            {
                continue;
            }

            // Main regex pattern - for well-structured lines
            ProcessLineWithMainPattern(line, readings);
        }

        // Second pass: Use a more flexible approach for remaining lines that might contain valid data
        foreach (var line in lines)
        {
            // Skip already processed lines or irrelevant lines
            if (string.IsNullOrWhiteSpace(line) ||
                line.Contains("Site Order") ||
                line.Contains("Grade Enterococci") ||
                line.Contains("RECREATIONAL") ||
                !ContainsPotentialSiteCode(line))
            {
                continue;
            }

            // Try with a more flexible pattern
            ProcessLineWithFlexiblePattern(line, readings);
        }
    }

    private bool ContainsPotentialSiteCode(string line)
    {
        // Check if line contains a potential site code pattern
        return Regex.IsMatch(line, @"(X?C[SN]\d+|HB\d+|CS\d+)");
    }

    private void ProcessLineWithMainPattern(string line, List<WaterQualityReading> readings)
    {
        // Updated regex pattern to better match table structure in the PDF
        var dataLineRegex = new Regex(@"^\s*(\d+)\s+(.*?)\s+(X?C[SN]\d+|HB\d+|CS\d+[A-Z]?)\s+([\d.-]+)\s+([-\d.]+)\s+(\w+)\s+(\d+)\s+(\w+)\s+(\w+)\s+([\d-]+)\s+(\d+)\s+(\w+)");

        var match = dataLineRegex.Match(line);
        if (match.Success)
        {
            try
            {
                // Extract the basic data fields
                var siteCode = match.Groups[3].Value.Trim();
                var dateStr = match.Groups[10].Value.Trim();
                var countStr = match.Groups[11].Value.Trim();

                // Parse date and count
                if (DateTime.TryParse(dateStr, out DateTime samplingDate) &&
                    double.TryParse(countStr, out double enterococcusCount))
                {
                    // Check if this beach code and date combination already exists
                    if (!readings.Any(r => r.BeachCode == siteCode && r.SamplingDate == samplingDate))
                    {
                        // Add the main reading
                        readings.Add(new WaterQualityReading
                        {
                            BeachCode = siteCode,
                            EnterococcusCount = enterococcusCount,

                            SamplingDate = DateTime.SpecifyKind(samplingDate, DateTimeKind.Utc),
                            SamplingFrequency = "Weekly"
                        });

                        // Find previous sample data that follows the main data fields
                        var remainingPart = line.Substring(match.Length).Trim();
                        if (!string.IsNullOrEmpty(remainingPart))
                        {
                            var previousSamples = Regex.Matches(remainingPart, @"\d+")
                                .Select(m => double.Parse(m.Value))
                                .ToList();

                            // For debugging only
                            Console.WriteLine($"Found {previousSamples.Count} previous samples for {siteCode}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the parsing error but continue processing other lines
                Console.WriteLine($"Error parsing line with main pattern: {line}. Error: {ex.Message}");
            }
        }
    }

    private void ProcessLineWithFlexiblePattern(string line, List<WaterQualityReading> readings)
    {
        try
        {
            // Extract the site code using a more flexible approach
            var siteCodeMatch = Regex.Match(line, @"(X?C[SN]\d+[A-Z]?|HB\d+|CS\d+[A-Z]?)");
            if (siteCodeMatch.Success)
            {
                var siteCode = siteCodeMatch.Groups[1].Value;

                // Try to find a date in the format yyyy-MM-dd
                var dateMatch = Regex.Match(line, @"(20\d{2}-\d{2}-\d{2})");
                if (dateMatch.Success)
                {
                    var dateStr = dateMatch.Groups[1].Value;

                    // Look for a number near the date that could be the count
                    // First, get the position of the date in the string
                    int datePosition = line.IndexOf(dateStr);

                    // Search for numbers in the vicinity of the date
                    var numberMatches = Regex.Matches(line, @"\d+");

                    // Find the closest number after the date
                    double? enterococcusCount = null;
                    int minDistance = int.MaxValue;

                    foreach (Match numMatch in numberMatches)
                    {
                        int numPos = numMatch.Index;
                        // Only consider numbers after the date and not part of the date itself
                        if (numPos > datePosition + dateStr.Length)
                        {
                            int distance = numPos - (datePosition + dateStr.Length);
                            if (distance < minDistance && double.TryParse(numMatch.Value, out double count))
                            {
                                minDistance = distance;
                                enterococcusCount = count;
                            }
                        }
                    }

                    if (enterococcusCount.HasValue &&
                        DateTime.TryParse(dateStr, out DateTime samplingDate))
                    {
                        // Check if this beach code and date combination already exists
                        if (!readings.Any(r => r.BeachCode == siteCode && r.SamplingDate == samplingDate))
                        {
                            readings.Add(new WaterQualityReading
                            {
                                BeachCode = siteCode,
                                EnterococcusCount = enterococcusCount.Value,
                                SamplingDate = DateTime.SpecifyKind(samplingDate, DateTimeKind.Utc),
                                SamplingFrequency = "Weekly"
                            });
                        }
                    }
                }
                else
                {
                    // Try alternative date formats
                    dateMatch = Regex.Match(line, @"(20\d{2}/\d{2}/\d{2})");
                    if (dateMatch.Success)
                    {
                        var dateStr = dateMatch.Groups[1].Value;

                        // Look for a number near the date that could be the count
                        // First, get the position of the date in the string
                        int datePosition = line.IndexOf(dateStr);

                        // Search for numbers in the vicinity of the date
                        var numberMatches = Regex.Matches(line, @"\d+");

                        // Find the closest number after the date
                        double? enterococcusCount = null;
                        int minDistance = int.MaxValue;

                        foreach (Match numMatch in numberMatches)
                        {
                            int numPos = numMatch.Index;
                            // Only consider numbers after the date and not part of the date itself
                            if (numPos > datePosition + dateStr.Length)
                            {
                                int distance = numPos - (datePosition + dateStr.Length);
                                if (distance < minDistance && double.TryParse(numMatch.Value, out double count))
                                {
                                    minDistance = distance;
                                    enterococcusCount = count;
                                }
                            }
                        }

                        if (enterococcusCount.HasValue &&
                            DateTime.TryParse(dateStr, out DateTime samplingDate))
                        {
                            // Check if this beach code and date combination already exists
                            if (!readings.Any(r => r.BeachCode == siteCode && r.SamplingDate == samplingDate))
                            {
                                readings.Add(new WaterQualityReading
                                {
                                    BeachCode = siteCode,
                                    EnterococcusCount = enterococcusCount.Value,
                                    SamplingDate = DateTime.SpecifyKind(samplingDate, DateTimeKind.Utc),
                                    SamplingFrequency = "Weekly"
                                });
                            }
                        }
                    }
                    // Try one more date format - Month day, year (e.g., "April 22, 2025")
                    else
                    {
                        dateMatch = Regex.Match(line, @"(January|February|March|April|May|June|July|August|September|October|November|December)\s+\d{1,2},\s+20\d{2}");
                        if (dateMatch.Success)
                        {
                            var dateStr = dateMatch.Groups[0].Value;

                            // Look for a number near the date that could be the count
                            int datePosition = line.IndexOf(dateStr);

                            // Search for numbers in the vicinity of the date
                            var numberMatches = Regex.Matches(line, @"\d+");

                            // Find the closest number after the date
                            double? enterococcusCount = null;
                            int minDistance = int.MaxValue;

                            foreach (Match numMatch in numberMatches)
                            {
                                int numPos = numMatch.Index;
                                // Only consider numbers after the date and not part of the date itself
                                if (numPos > datePosition + dateStr.Length)
                                {
                                    int distance = numPos - (datePosition + dateStr.Length);
                                    // Skip if the number is part of a year
                                    if (distance < minDistance &&
                                        double.TryParse(numMatch.Value, out double count) &&
                                        count < 10000) // Avoid counting years
                                    {
                                        minDistance = distance;
                                        enterococcusCount = count;
                                    }
                                }
                            }

                            if (enterococcusCount.HasValue &&
                                DateTime.TryParse(dateStr, out DateTime samplingDate))
                            {
                                // Check if this beach code and date combination already exists
                                if (!readings.Any(r => r.BeachCode == siteCode && r.SamplingDate == samplingDate))
                                {
                                    readings.Add(new WaterQualityReading
                                    {
                                        BeachCode = siteCode,
                                        EnterococcusCount = enterococcusCount.Value,
                                        SamplingDate = DateTime.SpecifyKind(samplingDate, DateTimeKind.Utc),
                                        SamplingFrequency = "Weekly"
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in flexible pattern parsing for line: {line}. Error: {ex.Message}");
        }
    }
}