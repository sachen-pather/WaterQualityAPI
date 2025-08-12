using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WaterQualityAPI.Models;
using WaterQualityAPI.Repositories;

namespace WaterQualityAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommunityController : ControllerBase
    {
        private readonly ICommunityPostRepository _repository;
        private readonly ILogger<CommunityController> _logger;

        public CommunityController(ICommunityPostRepository repository, ILogger<CommunityController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // GET api/Community?beachCode=XCN08
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetApprovedPosts([FromQuery] string beachCode)
        {
            try
            {
                var posts = await _repository.GetApprovedPostsByBeachCodeAsync(beachCode);

                // Transform data to match frontend expectations
                var formattedPosts = posts.Select(p => new
                {
                    id = p.Id,
                    post_id = p.Id, // For frontend compatibility
                    beachCode = p.BeachCode,
                    content = p.Content,
                    status = p.Status,
                    createdAt = p.CreatedAt,
                    created_at = p.CreatedAt, // For frontend compatibility
                    moderatedAt = p.ModeratedAt
                });

                return Ok(formattedPosts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving approved posts for beach {BeachCode}", beachCode);
                return StatusCode(500, "An error occurred while retrieving posts");
            }
        }

        // POST api/Community
        [HttpPost]
        public async Task<ActionResult> CreatePost([FromBody] CreatePostDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var post = new CommunityPost
                {
                    BeachCode = dto.BeachCode,
                    Content = dto.Content
                };

                await _repository.CreatePostAsync(post);

                return Ok(new
                {
                    status = "success",
                    message = "Post submitted for moderation"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                return StatusCode(500, "An error occurred while creating the post");
            }
        }

        // GET api/Community/pending
        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<object>>> GetPendingPosts()
        {
            try
            {
                var posts = await _repository.GetPendingPostsAsync();

                // Transform data to match frontend expectations
                var formattedPosts = posts.Select(p => new
                {
                    id = p.Id,
                    post_id = p.Id, // For frontend compatibility
                    beachCode = p.BeachCode,
                    content = p.Content,
                    status = p.Status,
                    createdAt = p.CreatedAt,
                    created_at = p.CreatedAt, // For frontend compatibility
                    moderatedAt = p.ModeratedAt
                });

                return Ok(formattedPosts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending posts");
                return StatusCode(500, "An error occurred while retrieving pending posts");
            }
        }

        // PUT api/Community/{postId}/approve
        [HttpPut("{postId}/approve")]
        public async Task<ActionResult> ApprovePost(int postId)
        {
            try
            {
                var result = await _repository.UpdatePostStatusAsync(postId, "approved");
                if (!result)
                {
                    return NotFound();
                }

                return Ok(new { status = "success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving post {PostId}", postId);
                return StatusCode(500, "An error occurred while approving the post");
            }
        }

        // PUT api/Community/{postId}/reject
        [HttpPut("{postId}/reject")]
        public async Task<ActionResult> RejectPost(int postId)
        {
            try
            {
                var result = await _repository.UpdatePostStatusAsync(postId, "rejected");
                if (!result)
                {
                    return NotFound();
                }

                return Ok(new { status = "success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting post {PostId}", postId);
                return StatusCode(500, "An error occurred while rejecting the post");
            }
        }
    }
}