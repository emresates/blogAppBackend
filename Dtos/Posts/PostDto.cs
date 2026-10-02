using System.Text.Json;
using BlogApi.Dtos.Categories;

namespace BlogApi.Dtos.Posts;

public class PostDto
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string Slug { get; set; } = "";

    public JsonElement Content { get; set; }

    public int ViewCount { get; set; }

    public int LikeCount { get; set; }

    public int CommentCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int UserId { get; set; }

    public string AuthorName { get; set; } = "";

    public List<CategoryDto> Categories { get; set; } = new();
}