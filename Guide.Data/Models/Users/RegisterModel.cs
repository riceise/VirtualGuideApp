using System.ComponentModel.DataAnnotations;

namespace Guide.Data.Models.Users;

public class RegisterModel
{
    [Required(ErrorMessage = "Поле Имя пользователя обязательно для заполнения.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Имя пользователя должно быть от 3 до 50 символов.")]
    public string UserName { get; set; } = string.Empty; 

    [Required(ErrorMessage = "Поле Email обязательно для заполнения.")]
    [EmailAddress(ErrorMessage = "Введите корректный Email адрес.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Поле Пароль обязательно для заполнения.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть не менее 6 символов.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Поле Подтверждение пароля обязательно.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Поле ФИО обязательно для заполнения")]
    public string FullName { get; set; } = string.Empty;
    
    public bool IsExcursionist { get; set; } = false;

}