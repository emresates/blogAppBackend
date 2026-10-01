using System.ComponentModel.DataAnnotations;

namespace BlogApi.Dtos.Categories;

public class UpdateCategoryDto
{
    [Required(ErrorMessage = "Kategori adı zorunludur.")]
    [MinLength(2, ErrorMessage = "Kategori adı en az 2 karakter olmalıdır.")]
    [MaxLength(50, ErrorMessage = "Kategori adı en fazla 50 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;
}