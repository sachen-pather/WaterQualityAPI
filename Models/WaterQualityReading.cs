using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WaterQualityAPI.Models
{
    [Table("water_quality_readings")]
    public class WaterQualityReading
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("beach_code")]
        public string BeachCode { get; set; } = string.Empty;

        [Required]
        [Column("sampling_date")]
        public DateTime SamplingDate { get; set; }

        [Column("enterococcus_count")]
        public double EnterococcusCount { get; set; }

        [Required]
        [Column("sampling_frequency")]
        public string SamplingFrequency { get; set; } = string.Empty;

        [Column("is_within_safety_threshold")]
        public bool IsWithinSafetyThreshold { get; set; }
    }
}