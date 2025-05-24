using Guide.Data.Models;

namespace Guide.Data.Models.AdminModels;

public class TourDetailsAdminDto
{
    public Guid TourId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Theme { get; set; }
    public TourStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public decimal? EstimatedDistanceMeters { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public string? CoverImageUrl { get; set; }
    
    public TourCreatorDto? CreatorUser { get; set; }
    public List<TourPointDetailsDto> TourPoints { get; set; } = new();
}

public class TourCreatorDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public class TourPointDetailsDto
{
    public int Order { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TextDescription { get; set; }
    public List<MediaContentDto> MediaContents { get; set; } = new();
}

public class MediaContentDto
{
    public int Order { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Title { get; set; }
}