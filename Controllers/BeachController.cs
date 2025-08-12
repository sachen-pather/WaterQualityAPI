using Microsoft.AspNetCore.Mvc;
using WaterQualityAPI.Models;
using WaterQualityAPI.Repositories;

namespace WaterQualityAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BeachController : ControllerBase
    {
        private readonly IBeachRepository _beachRepository;
        private readonly ILogger<BeachController> _logger;

        public BeachController(IBeachRepository beachRepository, ILogger<BeachController> logger)
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

        [HttpGet("{code}")]
        [ProducesResponseType(typeof(WaterQualityReading), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<WaterQualityReading>> GetBeach(string code)
        {
            try
            {
                var beach = await _beachRepository.GetBeachByCodeAsync(code);

                if (beach == null)
                {
                    return NotFound(new { message = $"Beach with code {code} not found" });
                }

                return Ok(beach);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving beach with code {code}");
                return StatusCode(500, new { message = "An error occurred while retrieving beach data", error = ex.Message });
            }
        }
    }
}