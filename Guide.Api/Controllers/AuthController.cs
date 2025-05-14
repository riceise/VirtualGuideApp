using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Guide.Data;
using Guide.Data.Models;
using Guide.Data.Models.Users;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<User> userManager,
        ApplicationDbContext context,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByNameAsync(model.UserName);

        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
        {
            _logger.LogInformation("Пользователь '{UserName}' вошёл в систему с ролью: {UserRole}", user.UserName, user.Role);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()) // Используем Role из модели User
            };

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YaTvoyMamyEbalDwaRazaNaToiNedele"));

            var token = new JwtSecurityToken(
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                userId = user.Id,
                userName = user.UserName,
                role = user.Role.ToString()
            });
        }

        _logger.LogWarning("Неудачная попытка входа для пользователя: {UserName}", model.UserName);
        return Unauthorized(new ErrorResponse { Message = "Неверное имя пользователя или пароль." });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userExists = await _userManager.FindByNameAsync(model.UserName);
        if (userExists != null)
        {
            return Conflict(new ErrorResponse { Message = "Пользователь с таким именем уже существует." });
        }

        var emailExists = await _userManager.FindByEmailAsync(model.Email);
        if (emailExists != null)
        {
            return Conflict(new ErrorResponse { Message = "Пользователь с таким Email уже существует." });
        }

        var user = new User
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.UserName,
            FullName = model.FullName,
            Role = model.IsExcursionist ? UserRole.Excursionist : UserRole.Tourist
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                _logger.LogError("Ошибка создания пользователя {UserName}: {Errors}", model.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));
                var errors = result.Errors.Select(e => e.Description).ToList();
                await transaction.RollbackAsync();
                return BadRequest(new ErrorResponse { Message = "Ошибка при создании пользователя.", Errors = errors });
            }

            _logger.LogInformation("Пользователь {UserName} ({UserId}) успешно создан с ролью {UserRole}", user.UserName, user.Id, user.Role);

            await transaction.CommitAsync();
            _logger.LogInformation("Регистрация пользователя {UserName} завершена успешно.", user.UserName);
            return Ok(new { Status = "Success", Message = "Пользователь успешно создан!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критическая ошибка во время регистрации пользователя {UserName}", model.UserName);
            try
            {
                await transaction.RollbackAsync();
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Ошибка при откате транзакции для пользователя {UserName}", model.UserName);
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Message = "Произошла внутренняя ошибка сервера при регистрации." });
        }
    }

    private class ErrorResponse
    {
        public string? Message { get; set; }
        public IEnumerable<string>? Errors { get; set; }
    }
}