namespace Guide.Data.Models.AdminModels;

public class AdminTourViewModel_Short
{
    public Guid TourId { get; set; }
    public string Title { get; set; } = string.Empty;
    public TourStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}