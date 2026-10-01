using System.ComponentModel.DataAnnotations;

namespace BlogApi.Dtos.Posts;

public class UpdatePostDto
{
    [Required(ErrorMessage = "titleIsRequired")]
    [MinLength(3, ErrorMessage = "titleMinLength")]
    [MaxLength(150, ErrorMessage = "titleMaxLength")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "contentIsRequired")]
    [MinLength(10, ErrorMessage = "contentMinLength")]
    public string Content { get; set; } = string.Empty;

    [MinLength(1, ErrorMessage = "categoryMinLength")]
    public List<int> CategoryIds { get; set; } = new();
}