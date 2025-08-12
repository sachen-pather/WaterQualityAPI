
// Repositories/ICommunityPostRepository.cs
using WaterQualityAPI.Models;

namespace WaterQualityAPI.Repositories
{
    public interface ICommunityPostRepository
    {
        // Get all approved posts for a specific beach
        Task<IEnumerable<CommunityPost>> GetApprovedPostsByBeachCodeAsync(string beachCode);

        // Create a new post
        Task<CommunityPost> CreatePostAsync(CommunityPost post);

        // Get all pending posts (for moderation)
        Task<IEnumerable<CommunityPost>> GetPendingPostsAsync();

        // Update post status (approve/reject)
        Task<bool> UpdatePostStatusAsync(int postId, string status);
    }
}