using Microsoft.AspNetCore.Mvc;
using WaterQualityAPI.Repositories;
using WaterQualityAPI.Services;
namespace WaterQualityAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly PdfParsingService _pdfParsingService;
        private readonly IWaterQualityRepository _waterQualityRepository;
        private readonly ILogger<UploadController> _logger;

        public UploadController(
            PdfParsingService pdfParsingService,
            IWaterQualityRepository waterQualityRepository,
            ILogger<UploadController> logger)
        {
            _pdfParsingService = pdfParsingService;
            _waterQualityRepository = waterQualityRepository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                _logger.LogInformation("Processing uploaded file: {FileName}, Size: {FileSize}",
                    file?.FileName, file?.Length);

                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("No file uploaded or empty file");
                    return BadRequest("No file uploaded or file is empty");
                }

                if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Invalid file type: {ContentType}", file.ContentType);
                    return BadRequest("Only PDF files are accepted");
                }

                // Create a memory stream to avoid file locking issues
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                _logger.LogInformation("Starting PDF parsing");
                var readings = await _pdfParsingService.ParsePdfAsync(memoryStream);

                if (readings.Count == 0)
                {
                    _logger.LogWarning("No readings extracted from the PDF");
                    return BadRequest("No water quality readings could be extracted from the PDF");
                }

                _logger.LogInformation("Successfully parsed {Count} readings", readings.Count);

                // Save readings to database
                await _waterQualityRepository.AddReadingsAsync(readings);
                _logger.LogInformation("Saved {Count} readings to database", readings.Count);

                return Ok(new
                {
                    message = $"Successfully processed {readings.Count} readings",
                    readingsCount = readings.Count,
                    beaches = readings.Select(r => r.BeachCode).Distinct().Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing uploaded file: {FileName}", file?.FileName);
                return StatusCode(500, new
                {
                    error = "Error processing the file",
                    details = ex.Message
                });
            }
        }
    }
}