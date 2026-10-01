namespace BlogApi.Dtos.Posts;
using BlogApi.Dtos.Categories;

public class PostDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int UserId { get; set; }

    public string AuthorName { get; set; } = string.Empty;

    public List<CategoryDto> Categories { get; set; } = new();
}