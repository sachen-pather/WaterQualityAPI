using System.ComponentModel.DataAnnotations;

namespace WaterQualityAPI.Models.DTOs
{
    public class CreateDiscussionDto
    {
        [Required]
        public required string Title { get; set; }

        [Required]
        public required string Content { get; set; }

        [Required]
        public required string Category { get; set; }
    }

    public class CreateCommentDto
    {
        [Required]
        public required string Content { get; set; }
    }
}