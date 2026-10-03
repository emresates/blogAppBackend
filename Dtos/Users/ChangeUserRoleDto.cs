using System.ComponentModel.DataAnnotations;

namespace BlogApi.Dtos.Users;

public class ChangeUserRoleDto
{
    [Required(ErrorMessage = "roleIsRequired")]
    public string Role { get; set; } = "";
}