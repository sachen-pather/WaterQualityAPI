using Dapper;
using Npgsql;
using WaterQualityAPI.Models;

namespace WaterQualityAPI.Repositories
{
    // Repositories/ICommunityRepository.cs
    public interface ICommunityRepository
    {
        Task<IEnumerable<CommunityDiscussion>> GetAllDiscussionsAsync();
        Task<CommunityDiscussion?> GetDiscussionByIdAsync(int id);
        Task<CommunityDiscussion> CreateDiscussionAsync(CommunityDiscussion discussion);
        Task<CommunityDiscussion> UpdateDiscussionAsync(CommunityDiscussion discussion);
        Task<bool> DeleteDiscussionAsync(int id);

        Task<IEnumerable<CommunityDiscussionPost>> GetAllPostsAsync();
        Task<CommunityDiscussionPost?> GetPostByIdAsync(int id);
        Task<CommunityDiscussionPost> CreatePostAsync(CommunityDiscussionPost post);
        Task<CommunityDiscussionPost> UpdatePostAsync(CommunityDiscussionPost post);
        Task<bool> DeletePostAsync(int id);

        Task<IEnumerable<CommunityComment>> GetCommentsByDiscussionIdAsync(int discussionId);
        Task<CommunityComment> AddCommentAsync(CommunityComment comment);
        Task<bool> DeleteCommentAsync(int commentId);
    }

    // Repositories/CommunityRepository.cs
    public class CommunityRepository : ICommunityRepository
    {
        private readonly string _connectionString;

        public CommunityRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SupabaseConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        private NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public async Task<IEnumerable<CommunityDiscussion>> GetAllDiscussionsAsync()
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<CommunityDiscussion>(
                "SELECT * FROM community_discussions ORDER BY created_at DESC");
        }

        public async Task<CommunityDiscussion?> GetDiscussionByIdAsync(int id)
        {
            using var connection = CreateConnection();
            var discussion = await connection.QueryFirstOrDefaultAsync<CommunityDiscussion>(
                "SELECT * FROM community_discussions WHERE id = @Id", new { Id = id });
            return discussion;
        }

        public async Task<CommunityDiscussion> CreateDiscussionAsync(CommunityDiscussion discussion)
        {
            using var connection = CreateConnection();
            var sql = @"
                INSERT INTO community_discussions (title, content, category, created_at) 
                VALUES (@Title, @Content, @Category, @CreatedAt) 
                RETURNING id";

            discussion.Id = await connection.QuerySingleAsync<int>(sql, discussion);
            return discussion;
        }

        public async Task<CommunityDiscussion> UpdateDiscussionAsync(CommunityDiscussion discussion)
        {
            using var connection = CreateConnection();
            var sql = @"
                UPDATE community_discussions 
                SET title = @Title, content = @Content, category = @Category 
                WHERE id = @Id";

            await connection.ExecuteAsync(sql, discussion);
            return discussion;
        }

        public async Task<bool> DeleteDiscussionAsync(int id)
        {
            using var connection = CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(
                "DELETE FROM community_discussions WHERE id = @Id", new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<CommunityDiscussionPost>> GetAllPostsAsync()
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<CommunityDiscussionPost>(
                "SELECT * FROM community_discussion_posts ORDER BY created_at DESC");
        }

        public async Task<CommunityDiscussionPost?> GetPostByIdAsync(int id)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<CommunityDiscussionPost>(
                "SELECT * FROM community_discussion_posts WHERE id = @Id", new { Id = id });
        }

        public async Task<CommunityDiscussionPost> CreatePostAsync(CommunityDiscussionPost post)
        {
            using var connection = CreateConnection();
            var sql = @"
                INSERT INTO community_discussion_posts (title, content, category, created_at) 
                VALUES (@Title, @Content, @Category, @CreatedAt) 
                RETURNING id";

            post.Id = await connection.QuerySingleAsync<int>(sql, post);
            return post;
        }

        public async Task<CommunityDiscussionPost> UpdatePostAsync(CommunityDiscussionPost post)
        {
            using var connection = CreateConnection();
            var sql = @"
                UPDATE community_discussion_posts 
                SET title = @Title, content = @Content, category = @Category 
                WHERE id = @Id";

            await connection.ExecuteAsync(sql, post);
            return post;
        }

        public async Task<bool> DeletePostAsync(int id)
        {
            using var connection = CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(
                "DELETE FROM community_discussion_posts WHERE id = @Id", new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<CommunityComment>> GetCommentsByDiscussionIdAsync(int discussionId)
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<CommunityComment>(
                "SELECT * FROM discussion_comments WHERE discussion_id = @DiscussionId ORDER BY created_at",
                new { DiscussionId = discussionId });
        }

        public async Task<CommunityComment> AddCommentAsync(CommunityComment comment)
        {
            using var connection = CreateConnection();
            var sql = @"
                INSERT INTO discussion_comments (discussion_id, content, created_at) 
                VALUES (@DiscussionId, @Content, @CreatedAt) 
                RETURNING id";

            comment.Id = await connection.QuerySingleAsync<int>(sql, comment);
            return comment;
        }

        public async Task<bool> DeleteCommentAsync(int id)
        {
            using var connection = CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(
                "DELETE FROM discussion_comments WHERE id = @Id", new { Id = id });
            return rowsAffected > 0;
        }
    }
}