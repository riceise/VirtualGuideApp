using System.ComponentModel.DataAnnotations;

namespace Guide.Data.Models.TourDTOs
{
    public class AddCommentRequest
    {
        [Required]
        [MinLength(1)]
        [MaxLength(1000)]
        public string Text { get; set; } = string.Empty;

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
    }
}