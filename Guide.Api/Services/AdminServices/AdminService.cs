using Guide.Data;
using Guide.Data.Models;
using Guide.Data.Models.AdminModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Guide.Services.AdminServices;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AdminService> _logger;

    public AdminService(ApplicationDbContext context, UserManager<User> userManager, ILogger<AdminService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<List<AdminTourViewModel>> GetAllToursAsync()
    {
        try
        {
            var tours = await _context.Tours
                .Include(t => t.CreatorUser)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new AdminTourViewModel
                {
                    TourId = t.TourId,
                    Title = t.Title,
                    CreatorFullName = t.CreatorUser.FullName,
                    CreatorUserId = t.CreatorUserId,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return tours;
        }
        catch (Exception ex)
        {
            // логирование ошибки
            Console.WriteLine("Ошибка в GetAllToursAsync: " + ex.Message);
            throw;
        }
    }


    public async Task<List<AdminUserViewModel>> GetAllUsersAsync()
    {
        return await _userManager.Users
            .Select(u => new AdminUserViewModel
            {
                UserId = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role
            })
            .ToListAsync();
    }

    public async Task<bool> UpdateTourStatusAsync(Guid tourId, TourStatus newStatus)
    {
        var tour = await _context.Tours.FindAsync(tourId);
        if (tour == null)
        {
            _logger.LogWarning("Попытка обновить статус несуществующего тура: {TourId}", tourId);
            return false;
        }

        tour.Status = newStatus;
        tour.UpdatedAt = DateTime.UtcNow;
        _context.Entry(tour).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Статус тура {TourId} обновлен на {NewStatus} через сервис.", tourId, newStatus);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Ошибка конкуренции при обновлении статуса тура {TourId} через сервис.", tourId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении изменений статуса тура {TourId} через сервис.", tourId);
            return false;
        }
    }

    public async Task<bool> DeleteTourAsync(Guid tourId)
    {
        var tour = await _context.Tours
            .Include(t => t.TourPoints)
            .ThenInclude(tp => tp.MediaContents)
            .FirstOrDefaultAsync(t => t.TourId == tourId);

        if (tour == null)
        {
            _logger.LogWarning("Попытка удалить несуществующий тур: {TourId} через сервис.", tourId);
            return false;
        }


        _context.Tours.Remove(tour);
        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Тур {TourId} удален через сервис.", tourId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении тура {TourId} через сервис.", tourId);
            return false;
        }
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> DeleteUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            _logger.LogWarning("Попытка удалить несуществующего пользователя: {UserId} через сервис.", userId);
            return (false, new[] { "Пользователь не найден." });
        }

        if (user.Role == UserRole.Administrator)
        {
            _logger.LogWarning("Попытка удалить администратора {UserId} через сервис.", userId);
            return (false, new[] { "Нельзя удалить пользователя с ролью 'Администратор'." });
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            _logger.LogInformation("Пользователь {UserId} удален через сервис.", userId);
            return (true, Enumerable.Empty<string>());
        }

        _logger.LogError("Ошибка удаления пользователя {UserId} через сервис: {Errors}", userId, string.Join(", ", result.Errors.Select(e => e.Description)));
        return (false, result.Errors.Select(e => e.Description));
    }

    public async Task<List<AdminTourViewModel>> GetToursByCreatorAsync(Guid? creatorId)
    {
        try
        {
            var query = _context.Tours
                .Include(t => t.CreatorUser)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new AdminTourViewModel
                {
                    TourId = t.TourId,
                    Title = t.Title,
                    CreatorFullName = t.CreatorUser.FullName,
                    CreatorUserId = t.CreatorUserId,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt
                });

            if (creatorId.HasValue)
            {
                query = query.Where(t => t.CreatorUserId == creatorId.Value);
            }

            var tours = await query.ToListAsync();
            _logger.LogInformation("Загружено {Count} туров для creatorId: {CreatorId}", tours.Count, creatorId?.ToString() ?? "все");
            return tours;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке туров для creatorId: {CreatorId}", creatorId?.ToString() ?? "все");
            return new List<AdminTourViewModel>();
        }
    }
    public async Task<UserDetailAdminDto> GetUserDetailsAsync(Guid userId)
    {
        try
        {
            var user = await _userManager.Users
                .Where(u => u.Id == userId)
                .Select(u => new UserDetailAdminDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    CreatedTours = u.CreatedTours.Select(t => new AdminTourViewModel_Short
                    {
                        TourId = t.TourId,
                        Title = t.Title,
                        Status = t.Status,
                        CreatedAt = t.CreatedAt
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                _logger.LogWarning("Пользователь с ID {UserId} не найден.", userId);
                return null;
            }

            _logger.LogInformation("Загружены детали пользователя {UserId} с {TourCount} турами.", userId, user.CreatedTours.Count);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке деталей пользователя {UserId}.", userId);
            return null;
        }
    }
    
    public async Task<TourDetailsAdminDto?> GetTourDetailsAsync(Guid tourId)
    {
        try
        {
            var tour = await _context.Tours
                .Include(t => t.CreatorUser)
                .Include(t => t.TourPoints)
                    .ThenInclude(tp => tp.MediaContents)
                .FirstOrDefaultAsync(t => t.TourId == tourId);

            if (tour == null)
            {
                _logger.LogWarning("Тур с ID {TourId} не найден", tourId);
                return null;
            }

            var dto = new TourDetailsAdminDto
            {
                TourId = tour.TourId,
                Title = tour.Title,
                Description = tour.Description,
                Theme = tour.Theme,
                Status = tour.Status,
                CreatedAt = tour.CreatedAt,
                UpdatedAt = tour.UpdatedAt,
                EstimatedDistanceMeters = tour.EstimatedDistanceMeters,
                EstimatedDurationMinutes = tour.EstimatedDurationMinutes,
                CoverImageUrl = tour.CoverImageUrl,
                CreatorUser = tour.CreatorUser != null ? new TourCreatorDto
                {
                    Id = tour.CreatorUser.Id,
                    UserName = tour.CreatorUser.UserName,
                    FullName = tour.CreatorUser.FullName
                } : null,
                TourPoints = tour.TourPoints.Select(tp => new TourPointDetailsDto
                {
                    Order = tp.Order,
                    Name = tp.Name,
                    TextDescription = tp.TextDescription,
                    MediaContents = tp.MediaContents.Select(mc => new MediaContentDto
                    {
                        Order = mc.Order,
                        Url = mc.Url,
                        Title = mc.Title
                    }).ToList()
                }).ToList()
            };

            _logger.LogInformation("Загружены детали тура {TourId}", tourId);
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке деталей тура {TourId}", tourId);
            throw;
        }
    }
}