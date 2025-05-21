using System.ComponentModel.DataAnnotations;

namespace Guide.Data.Models.TourDTOs;

public class CreateTourRequest
{
    [Required]
    [StringLength(255)]
    public string Title { get; set; }

    [StringLength(4000)] 
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Theme { get; set; }

    [Required]
    public List<TourPointRequest> Points { get; set; } = new List<TourPointRequest>();
}