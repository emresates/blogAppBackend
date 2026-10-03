using BlogApi.Constants;
using BlogApi.Data;
using BlogApi.Dtos.Users;
using BlogApi.Exceptions;
using BlogApi.Interfaces;
using BlogApi.Models;
using BlogApi.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(
        AppDbContext context
    )
    {
        _context = context;
    }

    public async Task<PagedResult<UserDto>> GetAllAsync(
        string? search,
        string? role,
        bool? isActive,
        int page,
        int pageSize
    )
    {
        var query = _context.Users
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchText =
                $"%{search.Trim()}%";

            query = query.Where(
                x =>
                    EF.Functions.ILike(
                        x.Name,
                        searchText
                    )
                    ||
                    EF.Functions.ILike(
                        x.Email,
                        searchText
                    )
            );
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(
                x => x.Role == role
            );
        }

        if (isActive.HasValue)
        {
            query = query.Where(
                x => x.IsActive == isActive.Value
            );
        }

        var totalCount =
            await query.CountAsync();

        var users = await query
            .OrderByDescending(
                x => x.CreatedAt
            )
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(
                x => new UserDto
                {
                    Id = x.Id,

                    Name = x.Name,

                    Email = x.Email,

                    Role = x.Role,

                    IsActive = x.IsActive,

                    CreatedAt = x.CreatedAt
                }
            )
            .ToListAsync();

        var totalPages =
            (int)Math.Ceiling(
                totalCount /
                (double)pageSize
            );

        return new PagedResult<UserDto>
        {
            Items = users,

            Pagination = new PaginationMeta
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            }
        };
    }

    public async Task<UserDto> GetByIdAsync(
        int id
    )
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == id
            );

        if (user == null)
        {
            throw new AppException(
                "Kullanıcı bulunamadı.",
                404,
                "userNotFound"
            );
        }

        return MapToDto(user);
    }

    public async Task<UserDto> ChangeRoleAsync(
        int targetUserId,
        string newRole,
        int currentUserId,
        string currentUserRole
    )
    {
        if (targetUserId == currentUserId)
        {
            throw new AppException(
                "Kendi rolünüzü değiştiremezsiniz.",
                400,
                "cannotChangeOwnRole"
            );
        }

        var normalizedRole =
            NormalizeRole(newRole);

        var targetUser =
            await _context.Users
                .FirstOrDefaultAsync(
                    x => x.Id == targetUserId
                );

        if (targetUser == null)
        {
            throw new AppException(
                "Kullanıcı bulunamadı.",
                404,
                "userNotFound"
            );
        }

        ValidateRoleChangePermission(
            currentUserRole,
            targetUser.Role,
            normalizedRole
        );

        targetUser.Role =
            normalizedRole;

        await _context.SaveChangesAsync();

        return MapToDto(targetUser);
    }

    public async Task<UserDto> ChangeStatusAsync(
        int targetUserId,
        bool isActive,
        int currentUserId,
        string currentUserRole
    )
    {
        if (targetUserId == currentUserId)
        {
            throw new AppException(
                "Kendi hesabınızı devre dışı bırakamazsınız.",
                400,
                "cannotDisableOwnAccount"
            );
        }

        var targetUser =
            await _context.Users
                .FirstOrDefaultAsync(
                    x => x.Id == targetUserId
                );

        if (targetUser == null)
        {
            throw new AppException(
                "Kullanıcı bulunamadı.",
                404,
                "userNotFound"
            );
        }

        ValidateTargetManagementPermission(
            currentUserRole,
            targetUser.Role
        );

        targetUser.IsActive =
            isActive;

        await _context.SaveChangesAsync();

        return MapToDto(targetUser);
    }

    private static void ValidateRoleChangePermission(
        string currentUserRole,
        string targetUserRole,
        string newRole
    )
    {
        if (currentUserRole == Roles.SuperAdmin)
        {
            return;
        }

        if (currentUserRole != Roles.Admin)
        {
            throw new AppException(
                "Bu işlem için yetkiniz yok.",
                403,
                "userManagementForbidden"
            );
        }

        if (
            targetUserRole == Roles.Admin ||
            targetUserRole == Roles.SuperAdmin
        )
        {
            throw new AppException(
                "Admin başka bir Admin veya SuperAdmin hesabını yönetemez.",
                403,
                "adminCannotManagePrivilegedUser"
            );
        }

        if (
            newRole == Roles.Admin ||
            newRole == Roles.SuperAdmin
        )
        {
            throw new AppException(
                "Admin, kullanıcıya Admin veya SuperAdmin rolü veremez.",
                403,
                "adminCannotGrantPrivilegedRole"
            );
        }
    }

    private static void ValidateTargetManagementPermission(
        string currentUserRole,
        string targetUserRole
    )
    {
        if (currentUserRole == Roles.SuperAdmin)
        {
            return;
        }

        if (currentUserRole != Roles.Admin)
        {
            throw new AppException(
                "Bu işlem için yetkiniz yok.",
                403,
                "userManagementForbidden"
            );
        }

        if (
            targetUserRole == Roles.Admin ||
            targetUserRole == Roles.SuperAdmin
        )
        {
            throw new AppException(
                "Admin başka bir Admin veya SuperAdmin hesabını yönetemez.",
                403,
                "adminCannotManagePrivilegedUser"
            );
        }
    }

    private static string NormalizeRole(
        string role
    )
    {
        if (
            role.Equals(
                Roles.User,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return Roles.User;
        }

        if (
            role.Equals(
                Roles.Author,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return Roles.Author;
        }

        if (
            role.Equals(
                Roles.Admin,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return Roles.Admin;
        }

        if (
            role.Equals(
                Roles.SuperAdmin,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return Roles.SuperAdmin;
        }

        throw new AppException(
            "Geçersiz kullanıcı rolü.",
            400,
            "invalidRole"
        );
    }

    private static UserDto MapToDto(
        User user
    )
    {
        return new UserDto
        {
            Id = user.Id,

            Name = user.Name,

            Email = user.Email,

            Role = user.Role,

            IsActive = user.IsActive,

            CreatedAt = user.CreatedAt
        };
    }
}