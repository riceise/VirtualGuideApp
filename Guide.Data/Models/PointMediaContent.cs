using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Guide.Data.Models;

public class PointMediaContent
{
    [Key]
    public Guid PointMediaContentId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid TourPointId { get; set; }
    [ForeignKey("TourPointId")]
    public virtual TourPoint TourPoint { get; set; }
    

    [Required]
    [StringLength(2048)]
    public string Url { get; set; } = string.Empty; 

    [StringLength(255)]
    public string? Title { get; set; } 
    
    public int Order { get; set; } = 0; 
}