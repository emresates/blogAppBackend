using BlogApi.Constants;

namespace BlogApi.Models;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Email { get; set; } = "";

    public string PasswordHash { get; set; } = "";

    public string Role { get; set; } = Roles.User;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Post> Posts { get; set; } = new();

    public List<PostLike> PostLikes { get; set; } = new();

    public List<Comment> Comments { get; set; } = new();

    public bool IsActive { get; set; } = true;
}