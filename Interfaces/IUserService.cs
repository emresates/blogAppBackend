using BlogApi.Dtos.Users;
using BlogApi.Models.Responses;

namespace BlogApi.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetAllAsync(
        string? search,
        string? role,
        bool? isActive,
        int page,
        int pageSize
    );

    Task<UserDto> GetByIdAsync(
        int id
    );

    Task<UserDto> ChangeRoleAsync(
        int targetUserId,
        string newRole,
        int currentUserId,
        string currentUserRole
    );

    Task<UserDto> ChangeStatusAsync(
        int targetUserId,
        bool isActive,
        int currentUserId,
        string currentUserRole
    );
}