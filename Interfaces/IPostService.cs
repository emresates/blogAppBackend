using BlogApi.Dtos.Posts;
using BlogApi.Models.Responses;

namespace BlogApi.Interfaces;

public interface IPostService
{
    Task<PagedResult<PostDto>> GetAllAsync(
        int? categoryId,
        string? search,
        int page,
        int pageSize
    );

    Task<PostDto> GetByIdAsync(int id);

    Task<PostDto> GetBySlugAsync(
        string slug
    );

    Task<PostDto> CreateAsync(
        CreatePostDto dto,
        int userId
    );

    Task<PostDto> UpdateAsync(
        int id,
        UpdatePostDto dto,
        int userId,
        string role
    );

    Task DeleteAsync(
        int id,
        int userId,
        string role
    );

    Task LikeAsync(
        int postId,
        int userId
    );

    Task UnlikeAsync(
        int postId,
        int userId
    );
}