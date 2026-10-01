using System.ComponentModel.DataAnnotations;
namespace BlogApi.Dtos.Posts;

public class CreatePostDto
{
    [Required(ErrorMessage = "Başlık zorunludur.")]
    [MinLength(3, ErrorMessage = "Başlık en az 3 karakter olmalıdır.")]
    [MaxLength(150, ErrorMessage = "Başlık en fazla 150 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "İçerik zorunludur.")]
    [MinLength(10, ErrorMessage = "İçerik en az 10 karakter olmalıdır.")]
    public string Content { get; set; } = string.Empty;

    [MinLength(1, ErrorMessage = "En az bir kategori seçmelisiniz.")]
    public List<int> CategoryIds { get; set; } = new();
}