using Microsoft.AspNetCore.Mvc;
using WaterQualityAPI.Models;
using WaterQualityAPI.Repositories;

[ApiController]
[Route("api/[controller]")]
public class BeachManagementController : ControllerBase
{
    private readonly IBeachRepository _beachRepository;
    private readonly ILogger<BeachManagementController> _logger;

    public BeachManagementController(IBeachRepository beachRepository, ILogger<BeachManagementController> logger)
    {
        _beachRepository = beachRepository;
        _logger = logger;
    }


    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WaterQualityReading>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<WaterQualityReading>>> GetAllBeaches()
    {
        try
        {
            var beaches = await _beachRepository.GetAllBeachesAsync();
            return Ok(beaches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving beaches");
            return StatusCode(500, new { message = "An error occurred while retrieving beach data", error = ex.Message });
        }
    }

    [HttpGet("{beachCode}")]
    [ProducesResponseType(typeof(WaterQualityReading), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WaterQualityReading>> GetBeach(string beachCode)
    {
        try
        {
            var beach = await _beachRepository.GetBeachByCodeAsync(beachCode);
            if (beach == null)
            {
                return NotFound(new { message = $"Beach with code {beachCode} not found" });
            }
            return Ok(beach);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving beach with code {beachCode}");
            return StatusCode(500, new { message = "An error occurred while retrieving beach data", error = ex.Message });
        }
    }
    [HttpPut("{beachCode}")]
    [ProducesResponseType(typeof(WaterQualityReading), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WaterQualityReading>> UpdateBeachInfo(string beachCode, [FromBody] WaterQualityReading reading)
    {
        if (beachCode != reading.BeachCode)
        {
            return BadRequest(new { message = "Beach code in URL does not match beach code in request body" });
        }

        try
        {
            var updatedReading = await _beachRepository.UpdateBeachReadingAsync(beachCode, reading);
            if (updatedReading == null)
            {
                return NotFound(new { message = $"Beach with code {beachCode} not found" });
            }
            return Ok(updatedReading);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating beach reading with code {beachCode}");
            return StatusCode(500, new { message = "An error occurred while updating beach reading", error = ex.Message });
        }
    }
}