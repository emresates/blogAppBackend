using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace BlogApi.Dtos.Posts;

public class CreatePostDto
{
    [Required(ErrorMessage = "titleIsRequired")]
    [MinLength(3, ErrorMessage = "titleMinLength")]
    [MaxLength(150, ErrorMessage = "titleMaxLength")]
    public string Title { get; set; } = "";

    public JsonElement Content { get; set; }

    [MinLength(1, ErrorMessage = "categoryIsRequired")]
    public List<int> CategoryIds { get; set; } = new();
}