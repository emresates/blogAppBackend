using System.ComponentModel.DataAnnotations;

namespace BlogApi.Dtos.Categories;

public class CreateCategoryDto
{
    [Required(ErrorMessage = "categoryNameIsRequired")]
    [MinLength(2, ErrorMessage = "categoryNameMinLength")]
    [MaxLength(50, ErrorMessage = "categoryNameMaxLength")]
    public string Name { get; set; } = string.Empty;
}