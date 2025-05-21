using System.ComponentModel.DataAnnotations;

namespace Guide.Data.Models.TourDTOs;

public class TourPointRequest
{
    [StringLength(255)]
    public string? Name { get; set; }

    [StringLength(2000)]
    public string? TextDescription { get; set; }

    [Required]
    public decimal Latitude { get; set; }

    [Required]
    public decimal Longitude { get; set; }

    public int Order { get; set; }

    public List<string> TempImageReferences { get; set; } = new List<string>();
    
}