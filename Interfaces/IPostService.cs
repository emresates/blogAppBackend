using BlogApi.Dtos.Posts;

namespace BlogApi.Interfaces;

public interface IPostService
{
    Task<List<PostDto>> GetAllAsync(int? categoryId);

    Task<PostDto> GetByIdAsync(int id);

    Task<PostDto> CreateAsync(
        CreatePostDto dto,
        int userId
    );

    Task<PostDto> UpdateAsync(
        int id,
        UpdatePostDto dto,
        int userId
    );

    Task DeleteAsync(
        int id,
        int userId
    );
}