using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WaterQualityAPI.Models
{
    [Table("community_posts")]
    public class CommunityPost
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("beach_code")]
        public string BeachCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        [Column("content")]
        public string Content { get; set; } = string.Empty;

        [Column("status")]
        public string Status { get; set; } = "pending";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("moderated_at")]
        public DateTime? ModeratedAt { get; set; }
    }

    public class CreatePostDto
    {
        public required string BeachCode { get; set; }
        public required string Content { get; set; }
    }
}