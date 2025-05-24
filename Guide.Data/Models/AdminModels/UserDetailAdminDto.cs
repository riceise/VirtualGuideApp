namespace Guide.Data.Models.AdminModels;

public class UserDetailAdminDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<AdminTourViewModel_Short> CreatedTours { get; set; } = new();
}