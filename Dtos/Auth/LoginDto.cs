using System.ComponentModel.DataAnnotations;

namespace BlogApi.Dtos.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "emailIsRequired")]
    [EmailAddress(ErrorMessage = "emailValidationFailed")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "passwordIsRequired")]
    [MinLength(6, ErrorMessage = "passwordMinLength")]
    [MaxLength(100, ErrorMessage = "passwordMaxLength")]
    public string Password { get; set; } = string.Empty;
}