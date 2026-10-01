namespace BlogApi.Dtos.Auth;

using System.ComponentModel.DataAnnotations;
public class RegisterDto
{
    [Required(ErrorMessage = "nameIsRequired")]
    [MinLength(2, ErrorMessage = "nameMinLength")]
    [MaxLength(50, ErrorMessage = "nameMaxLength")]
    public string Name { get; set; } = string.Empty;


    [Required(ErrorMessage = "emailIsRequired")]
    [EmailAddress(ErrorMessage = "emailValidationFailed")]
    public string Email { get; set; } = string.Empty;


    [Required(ErrorMessage = "passwordIsRequired")]
    [MinLength(6, ErrorMessage = "passwordMinLength")]
    [MaxLength(100, ErrorMessage = "passwordMaxLength")]
    public string Password { get; set; } = string.Empty;
}