namespace Guide.Data.Models.AdminModels;

public class AdminTourViewModel
{
    public Guid TourId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CreatorFullName { get; set; } = string.Empty;
    public Guid CreatorUserId { get; set; } 
    public TourStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}