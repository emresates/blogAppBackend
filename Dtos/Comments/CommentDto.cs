namespace BlogApi.Dtos.Comments;

public class CommentDto
{
    public int Id { get; set; }

    public string Content { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int UserId { get; set; }

    public string UserName { get; set; } = "";

    public int PostId { get; set; }

    public int? ParentCommentId { get; set; }

    public List<CommentDto> Replies { get; set; } = new();
}