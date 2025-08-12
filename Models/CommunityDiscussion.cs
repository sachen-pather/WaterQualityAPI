using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WaterQualityAPI.Models
{
    [Table("community_discussions")]
    public class CommunityDiscussion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("title")]
        public required string Title { get; set; }

        [Required]
        [Column("content")]
        public required string Content { get; set; }

        [Required]
        [Column("category")]
        public required string Category { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("community_comments")]
    public class CommunityComment
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("discussion_id")]
        public required int DiscussionId { get; set; }

        [Required]
        [Column("content")]
        public required string Content { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // THIS IS THE MISSING PROPERTY THAT WAS CAUSING THE ERROR
        [ForeignKey("DiscussionId")]
        public virtual CommunityDiscussion? Discussion { get; set; }
    }

    [Table("community_discussion_posts")]
    public class CommunityDiscussionPost
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("title")]
        public required string Title { get; set; }

        [Required]
        [Column("content")]
        public required string Content { get; set; }

        [Required]
        [Column("category")]
        public required string Category { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}