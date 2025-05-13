using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Guide.Data.Models;

public class TourPoint
{
    [Key] public Guid TourPointId { get; set; } = Guid.NewGuid();

    [Required] public Guid TourId { get; set; }
    [ForeignKey("TourId")] public virtual Tour Tour { get; set; }

    [StringLength(255)] public string? Name { get; set; }

    [Column(TypeName = "nvarchar(MAX)")] public string? TextDescription { get; set; }

    [Required] public int Order { get; set; }

    [Required]
    [Column(TypeName = "decimal(9,6)")]
    public decimal Latitude { get; set; }

    [Required]
    [Column(TypeName = "decimal(9,6)")]
    public decimal Longitude { get; set; }

    public virtual ICollection<PointMediaContent> MediaContents { get; set; } = new List<PointMediaContent>(); 

}