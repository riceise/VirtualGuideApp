using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Guide.Data.Models;
public enum TourStatus
{
    Draft,
    PendingModeration,
    Approved,
    Rejected,
    Archived
}

public class Tour
{
    [Key]
    public Guid TourId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CreatorUserId { get; set; }
    [ForeignKey("CreatorUserId")]
    public virtual User CreatorUser { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [StringLength(100)]
    public string? Theme { get; set; }

    [StringLength(2048)]
    public string? CoverImageUrl { get; set; }

    [Required]
    public TourStatus Status { get; set; } = TourStatus.Draft;

    public int? EstimatedDurationMinutes { get; set; }
    public int? EstimatedDistanceMeters { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<TourPoint> TourPoints { get; set; } = new List<TourPoint>();
}