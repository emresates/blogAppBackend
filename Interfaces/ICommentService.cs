using BlogApi.Dtos.Comments;

namespace BlogApi.Interfaces;

public interface ICommentService
{
    Task<List<CommentDto>> GetByPostIdAsync(
        int postId
    );

    Task<CommentDto> CreateAsync(
        int postId,
        CreateCommentDto dto,
        int userId
    );

    Task<CommentDto> UpdateAsync(
        int commentId,
        UpdateCommentDto dto,
        int userId,
        string role
    );

    Task DeleteAsync(
        int commentId,
        int userId,
        string role
    );
}