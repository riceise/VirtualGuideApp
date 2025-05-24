namespace Guide.Data.Models.AdminModels;

public class AdminUserViewModel
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; } 
    public UserRole Role { get; set; }
    public string? UserName { get; set; }

}