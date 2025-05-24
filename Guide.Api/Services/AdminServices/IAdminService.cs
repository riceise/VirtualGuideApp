using Guide.Data.Models;
using Guide.Data.Models.AdminModels;

namespace Guide.Services.AdminServices;

public interface IAdminService
{
    Task<List<AdminTourViewModel>> GetAllToursAsync();
    Task<List<AdminUserViewModel>> GetAllUsersAsync();
    Task<bool> UpdateTourStatusAsync(Guid tourId, TourStatus newStatus);
    Task<bool> DeleteTourAsync(Guid tourId);
    Task<(bool Success, IEnumerable<string> Errors)> DeleteUserAsync(Guid userId);
    Task<List<AdminTourViewModel>> GetToursByCreatorAsync(Guid? creatorId); 
    Task<UserDetailAdminDto> GetUserDetailsAsync(Guid userId);
}