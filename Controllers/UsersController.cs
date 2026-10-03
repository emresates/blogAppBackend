using System.Security.Claims;
using BlogApi.Dtos.Users;
using BlogApi.Exceptions;
using BlogApi.Interfaces;
using BlogApi.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(
        IUserService userService
    )
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? role,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20
    )
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 1;
        }

        if (pageSize > 100)
        {
            pageSize = 100;
        }

        var result =
            await _userService.GetAllAsync(
                search,
                role,
                isActive,
                page,
                pageSize
            );

        return Ok(
            ApiResponse<List<UserDto>>
                .Success(
                    result.Items,
                    200,
                    "Kullanıcılar getirildi.",
                    result.Pagination
                )
        );
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id
    )
    {
        var result =
            await _userService.GetByIdAsync(id);

        return Ok(
            ApiResponse<UserDto>.Success(
                result,
                200,
                "Kullanıcı getirildi."
            )
        );
    }

    [HttpPatch("{id:int}/role")]
    public async Task<IActionResult> ChangeRole(
        int id,
        ChangeUserRoleDto dto
    )
    {
        var currentUserId =
            GetCurrentUserId();

        var currentUserRole =
            GetCurrentUserRole();

        var result =
            await _userService.ChangeRoleAsync(
                id,
                dto.Role,
                currentUserId,
                currentUserRole
            );

        return Ok(
            ApiResponse<UserDto>.Success(
                result,
                200,
                "Kullanıcı rolü güncellendi."
            )
        );
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(
        int id,
        ChangeUserStatusDto dto
    )
    {
        var currentUserId =
            GetCurrentUserId();

        var currentUserRole =
            GetCurrentUserRole();

        var result =
            await _userService.ChangeStatusAsync(
                id,
                dto.IsActive,
                currentUserId,
                currentUserRole
            );

        return Ok(
            ApiResponse<UserDto>.Success(
                result,
                200,
                dto.IsActive
                    ? "Kullanıcı aktif edildi."
                    : "Kullanıcı devre dışı bırakıldı."
            )
        );
    }

    private int GetCurrentUserId()
    {
        var value =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        if (
            !int.TryParse(
                value,
                out var userId
            )
        )
        {
            throw new AppException(
                "Geçersiz kullanıcı.",
                401,
                "invalidToken"
            );
        }

        return userId;
    }

    private string GetCurrentUserRole()
    {
        var role =
            User.FindFirstValue(
                ClaimTypes.Role
            );

        if (
            string.IsNullOrWhiteSpace(role)
        )
        {
            throw new AppException(
                "Kullanıcı rolü bulunamadı.",
                401,
                "invalidToken"
            );
        }

        return role;
    }
}