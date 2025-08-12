using Microsoft.EntityFrameworkCore;
using WaterQualityAPI.Data;
using WaterQualityAPI.Models;
namespace WaterQualityAPI.Repositories
{
    public class CommunityPostRepository : ICommunityPostRepository
    {
        private readonly SupabaseContext _context;

        public CommunityPostRepository(SupabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CommunityPost>> GetApprovedPostsByBeachCodeAsync(string beachCode)
        {
            return await _context.CommunityPosts
                .Where(p => p.BeachCode == beachCode && p.Status == "approved")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<CommunityPost> CreatePostAsync(CommunityPost post)
        {
            post.CreatedAt = DateTime.UtcNow;
            post.Status = "pending"; // All new posts start as pending
            _context.CommunityPosts.Add(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<IEnumerable<CommunityPost>> GetPendingPostsAsync()
        {
            return await _context.CommunityPosts
                .Where(p => p.Status == "pending")
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdatePostStatusAsync(int postId, string status)
        {
            var post = await _context.CommunityPosts.FindAsync(postId);
            if (post == null) return false;
            post.Status = status;
            post.ModeratedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}