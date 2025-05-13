// Guide.Controllers.AuthController.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc; 
using Microsoft.EntityFrameworkCore;
using Guide.Data;
using Guide.Data.Models; 
using Guide.Data.Models.Users; 
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration; 
using Microsoft.Extensions.Logging; 
using Microsoft.AspNetCore.Http; 

namespace Guide.Controllers 
{
    [Route("api/[controller]")]
    [ApiController] 
    public class AuthController : ControllerBase 
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration; 
        private readonly RoleManager<IdentityRole<Guid>> _roleManager; 

        // Роли из вашего ТЗ
        private const string RoleTourist = "Tourist";
        private const string RoleExcursionist = "Excursionist";
        private const string RoleAdministrator = "Administrator";


        public AuthController(
            UserManager<User> userManager,
            ApplicationDbContext context,
            ILogger<AuthController> logger,
            IConfiguration configuration,
            RoleManager<IdentityRole<Guid>> roleManager) 
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _roleManager = roleManager;
        }

        [HttpPost("login")] 
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                _logger.LogInformation("Пользователь '{UserName}' вошёл в систему с ролями: {UserRoles}", user.UserName, string.Join(", ", userRoles));

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"];
                var issuer = jwtSettings["Issuer"];
                var audience = jwtSettings["Audience"];
                var expirationHours = jwtSettings.GetValue<int>("ExpirationHours", 3);


                if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
                {
                    _logger.LogError("JWT SecretKey не настроен или слишком короткий.");
                    return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Message = "Ошибка конфигурации сервера аутентификации." });
                }
                if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
                {
                    _logger.LogError("JWT Issuer или Audience не настроены.");
                    return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Message = "Ошибка конфигурации сервера аутентификации." });
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    expires: DateTime.Now.AddHours(expirationHours),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    userId = user.Id,
                    userName = user.UserName,
                    roles = userRoles
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

            var user = new User()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName,
                FullName = model.FullName,
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

                string roleToAssign = model.IsExcursionist ? RoleExcursionist : RoleTourist;

                if (!await _roleManager.RoleExistsAsync(roleToAssign))
                {
                    _logger.LogError("Роль '{RoleName}' не найдена в системе.", roleToAssign);
                    await transaction.RollbackAsync();
                    return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Message = "Ошибка конфигурации ролей." });
                }

                var addToRoleResult = await _userManager.AddToRoleAsync(user, roleToAssign);
                if (!addToRoleResult.Succeeded)
                {
                    _logger.LogError("Ошибка назначения роли '{RoleName}' пользователю {UserName}: {Errors}",
                        roleToAssign, model.UserName, string.Join(", ", addToRoleResult.Errors.Select(e => e.Description)));
                    var errors = addToRoleResult.Errors.Select(e => e.Description).ToList();
                    await transaction.RollbackAsync();
                    return BadRequest(new ErrorResponse { Message = "Ошибка при назначении роли пользователю.", Errors = errors });
                }

                _logger.LogInformation("Пользователь {UserName} ({UserId}) успешно создан с ролью {UserRole}", user.UserName, user.Id, roleToAssign);


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
}