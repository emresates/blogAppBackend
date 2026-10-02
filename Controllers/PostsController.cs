using System.Security.Claims;
using BlogApi.Dtos.Posts;
using BlogApi.Exceptions;
using BlogApi.Interfaces;
using BlogApi.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers;

[ApiController]
[Route("api/posts")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(
        IPostService postService
    )
    {
        _postService = postService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? categoryId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
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
            await _postService.GetAllAsync(
                categoryId,
                search,
                page,
                pageSize
            );

        return Ok(
            ApiResponse<List<PostDto>>
                .Success(
                    result.Items,
                    200,
                    "Postlar getirildi.",
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
            await _postService.GetByIdAsync(id);

        return Ok(
            ApiResponse<PostDto>
                .Success(
                    result,
                    200,
                    "Post getirildi."
                )
        );
    }

    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(
    string slug
)
    {
        var result =
            await _postService
                .GetBySlugAsync(slug);

        return Ok(
            ApiResponse<PostDto>.Success(
                result,
                200,
                "Post getirildi."
            )
        );
    }

    [Authorize(
        Roles = "Author,Admin,SuperAdmin"
    )]
    [HttpPost]
    public async Task<IActionResult> Create(
        CreatePostDto dto
    )
    {
        var userId = GetUserId();

        var result =
            await _postService.CreateAsync(
                dto,
                userId
            );

        return StatusCode(
            201,
            ApiResponse<PostDto>
                .Success(
                    result,
                    201,
                    "Post oluşturuldu."
                )
        );
    }

    [Authorize(
        Roles = "Author,Admin,SuperAdmin"
    )]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdatePostDto dto
    )
    {
        var userId = GetUserId();

        var role = GetUserRole();

        var result =
            await _postService.UpdateAsync(
                id,
                dto,
                userId,
                role
            );

        return Ok(
            ApiResponse<PostDto>
                .Success(
                    result,
                    200,
                    "Post güncellendi."
                )
        );
    }

    [Authorize(
        Roles = "Author,Admin,SuperAdmin"
    )]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id
    )
    {
        var userId = GetUserId();

        var role = GetUserRole();

        await _postService.DeleteAsync(
            id,
            userId,
            role
        );

        return Ok(
            ApiResponse<object>
                .Success(
                    new { },
                    200,
                    "Post silindi."
                )
        );
    }

    [Authorize]
    [HttpPost("{id:int}/like")]
    public async Task<IActionResult> Like(
        int id
    )
    {
        var userId = GetUserId();

        await _postService.LikeAsync(
            id,
            userId
        );

        return Ok(
            ApiResponse<object>
                .Success(
                    new { },
                    200,
                    "Post beğenildi."
                )
        );
    }

    [Authorize]
    [HttpDelete("{id:int}/like")]
    public async Task<IActionResult> Unlike(
        int id
    )
    {
        var userId = GetUserId();

        await _postService.UnlikeAsync(
            id,
            userId
        );

        return Ok(
            ApiResponse<object>
                .Success(
                    new { },
                    200,
                    "Beğeni kaldırıldı."
                )
        );
    }

    private int GetUserId()
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        if (
            !int.TryParse(
                userId,
                out var parsedUserId
            )
        )
        {
            throw new AppException(
                "Geçersiz kullanıcı.",
                401,
                "invalidToken"
            );
        }

        return parsedUserId;
    }

    private string GetUserRole()
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