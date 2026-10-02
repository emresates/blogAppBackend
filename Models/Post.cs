namespace BlogApi.Models;

public class Post
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string Content { get; set; } = "";

    public int ViewCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public List<Category> Categories { get; set; } = new();

    public List<PostLike> Likes { get; set; } = new();
}