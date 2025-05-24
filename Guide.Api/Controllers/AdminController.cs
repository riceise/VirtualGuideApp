using Guide.Data.Models;
using Guide.Data.Models.AdminModels;
using Guide.Services.AdminServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guide.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Administrator")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public class TourStatusUpdateRequest
    {
        public TourStatus Status { get; set; }
    }

    public AdminController(IAdminService adminService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    // GET: api/Admin/tours
    [HttpGet("tours")]
    public async Task<ActionResult<IEnumerable<AdminTourViewModel>>> GetAllTours()
    {
        var tours = await _adminService.GetAllToursAsync();
        return Ok(tours);
    }

    // PUT: api/Admin/tours/{tourId}/status
    [HttpPut("tours/{tourId}/status")]
    public async Task<IActionResult> UpdateTourStatus(Guid tourId, [FromBody] TourStatusUpdateRequest request)
    {
        var success = await _adminService.UpdateTourStatusAsync(tourId, request.Status);
        if (!success)
        {
            return NotFound("Маршрут не найден или произошла ошибка при обновлении.");
        }
        return NoContent();
    }

    // DELETE: api/Admin/tours/{tourId}
    [HttpDelete("tours/{tourId}")]
    public async Task<IActionResult> DeleteTour(Guid tourId)
    {
        var success = await _adminService.DeleteTourAsync(tourId);
        if (!success)
        {
            return NotFound("Маршрут не найден или произошла ошибка при удалении.");
        }
        return NoContent();
    }

    // GET: api/Admin/users
    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<AdminUserViewModel>>> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        return Ok(users);
    }

    // DELETE: api/Admin/users/{userId}
    [HttpDelete("users/{userId}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        var (success, errors) = await _adminService.DeleteUserAsync(userId);
        if (!success)
        {
            if (errors.Any(e => e.Contains("Пользователь не найден")))
                return NotFound(string.Join(", ", errors));
            if (errors.Any(e => e.Contains("Нельзя удалить пользователя")))
                return BadRequest(string.Join(", ", errors));
            
            return BadRequest(errors); 
        }
        return NoContent();
    }

    // GET: api/Admin/tours?creatorId={creatorId}
    [HttpGet("tours/by-creator")]
    public async Task<ActionResult<IEnumerable<AdminTourViewModel>>> GetToursByCreator([FromQuery] Guid? creatorId)
    {
        var tours = await _adminService.GetToursByCreatorAsync(creatorId);
        return Ok(tours);
    }
    
    [HttpGet("users/{userId}/full-details")]
    public async Task<ActionResult<UserDetailAdminDto>> GetUserDetails(Guid userId)
    {
        var userDetails = await _adminService.GetUserDetailsAsync(userId);
        if (userDetails == null)
        {
            return NotFound("Пользователь не найден.");
        }
        return Ok(userDetails);
    }
    
    [HttpGet("tours/{tourId}/full-details")]
    public async Task<ActionResult<TourDetailsAdminDto>> GetTourDetails(Guid tourId)
    {
        var tour = await _adminService.GetTourDetailsAsync(tourId);
        
        if (tour == null)
        {
            return NotFound("Маршрут не найден");
        }

        return Ok(tour);
    }
        
}