using Microsoft.AspNetCore.Mvc;
using WaterQualityAPI.Models;
using WaterQualityAPI.Models.DTOs;
using WaterQualityAPI.Repositories;

namespace WaterQualityAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CommunityDiscussionController : ControllerBase
    {
        private readonly ILogger<CommunityDiscussionController> _logger;
        private readonly ICommunityRepository _repository;

        public CommunityDiscussionController(
            ILogger<CommunityDiscussionController> logger,
            ICommunityRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        /// <summary>
        /// Gets all discussions
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CommunityDiscussion>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CommunityDiscussion>>> GetDiscussions()
        {
            try
            {
                var discussions = await _repository.GetAllDiscussionsAsync();
                return Ok(discussions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving discussions: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while retrieving discussions" });
            }
        }

        /// <summary>
        /// Gets a specific discussion
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CommunityDiscussion), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CommunityDiscussion>> GetDiscussion(int id)
        {
            try
            {
                var discussion = await _repository.GetDiscussionByIdAsync(id);
                if (discussion == null)
                    return NotFound(new { message = "Discussion not found" });

                return Ok(discussion);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving discussion {id}: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while retrieving the discussion" });
            }
        }

        /// <summary>
        /// Creates a new discussion
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CommunityDiscussion), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CommunityDiscussion>> CreateDiscussion([FromBody] CreateDiscussionDto dto)
        {
            try
            {
                var discussion = new CommunityDiscussion
                {
                    Title = dto.Title,
                    Content = dto.Content,
                    Category = dto.Category,
                    CreatedAt = DateTime.UtcNow
                };

                var created = await _repository.CreateDiscussionAsync(discussion);
                return CreatedAtAction(nameof(GetDiscussion), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating discussion: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while creating the discussion" });
            }
        }

        /// <summary>
        /// Gets comments for a discussion
        /// </summary>
        [HttpGet("{discussionId}/comments")]
        [ProducesResponseType(typeof(IEnumerable<CommunityComment>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CommunityComment>>> GetComments(int discussionId)
        {
            try
            {
                var discussion = await _repository.GetDiscussionByIdAsync(discussionId);
                if (discussion == null)
                    return NotFound(new { message = "Discussion not found" });

                var comments = await _repository.GetCommentsByDiscussionIdAsync(discussionId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving comments for discussion {discussionId}: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while retrieving comments" });
            }
        }

        /// <summary>
        /// Adds a comment to a discussion
        /// </summary>
        [HttpPost("{discussionId}/comments")]
        [ProducesResponseType(typeof(CommunityComment), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CommunityComment>> AddComment(int discussionId, [FromBody] CreateCommentDto dto)
        {
            try
            {
                var discussion = await _repository.GetDiscussionByIdAsync(discussionId);
                if (discussion == null)
                    return NotFound(new { message = "Discussion not found" });

                var comment = new CommunityComment
                {
                    DiscussionId = discussionId,
                    Content = dto.Content,
                    CreatedAt = DateTime.UtcNow
                };

                var created = await _repository.AddCommentAsync(comment);
                return CreatedAtAction(nameof(GetComments), new { discussionId }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating comment for discussion {discussionId}: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while creating the comment" });
            }
        }
    }
}