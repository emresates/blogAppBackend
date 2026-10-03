using System.ComponentModel.DataAnnotations;

namespace BlogApi.Dtos.Comments;

public class CreateReplyDto
{
    [Required(ErrorMessage = "commentIsRequired")]
    [MinLength(1, ErrorMessage = "commentMinLength")]
    [MaxLength(1000, ErrorMessage = "commentMaxLength")]
    public string Content { get; set; } = "";
}