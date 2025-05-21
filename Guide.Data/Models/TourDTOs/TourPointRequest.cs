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

    // Файлы изображений будут загружаться отдельно, здесь могут быть временные ID или имена файлов
    public List<string> TempImageReferences { get; set; } = new List<string>();
    
}