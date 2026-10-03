using System.Security.Claims;
using BlogApi.Dtos.Comments;
using BlogApi.Exceptions;
using BlogApi.Interfaces;
using BlogApi.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers;

[ApiController]
[Route("api")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(
        ICommentService commentService
    )
    {
        _commentService = commentService;
    }

    [HttpGet("posts/{postId:int}/comments")]
    public async Task<IActionResult> GetByPostId(
        int postId
    )
    {
        var result =
            await _commentService
                .GetByPostIdAsync(postId);

        return Ok(
            ApiResponse<List<CommentDto>>
                .Success(
                    result,
                    200,
                    "Yorumlar getirildi."
                )
        );
    }

    [Authorize]
    [HttpPost("posts/{postId:int}/comments")]
    public async Task<IActionResult> Create(
        int postId,
        CreateCommentDto dto
    )
    {
        var userId = GetUserId();

        var result =
            await _commentService.CreateAsync(
                postId,
                dto,
                userId
            );

        return StatusCode(
            201,
            ApiResponse<CommentDto>
                .Success(
                    result,
                    201,
                    "Yorum oluşturuldu."
                )
        );
    }

    [Authorize]
    [HttpPut("comments/{commentId:int}")]
    public async Task<IActionResult> Update(
        int commentId,
        UpdateCommentDto dto
    )
    {
        var userId = GetUserId();

        var role = GetUserRole();

        var result =
            await _commentService.UpdateAsync(
                commentId,
                dto,
                userId,
                role
            );

        return Ok(
            ApiResponse<CommentDto>
                .Success(
                    result,
                    200,
                    "Yorum güncellendi."
                )
        );
    }

    [Authorize]
    [HttpDelete("comments/{commentId:int}")]
    public async Task<IActionResult> Delete(
        int commentId
    )
    {
        var userId = GetUserId();

        var role = GetUserRole();

        await _commentService.DeleteAsync(
            commentId,
            userId,
            role
        );

        return Ok(
            ApiResponse<object>
                .Success(
                    new { },
                    200,
                    "Yorum silindi."
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

    [Authorize]
    [HttpPost("comments/{commentId:int}/replies")]
    public async Task<IActionResult> CreateReply(
    int commentId,
    CreateReplyDto dto
)
    {
        var userId =
            GetUserId();

        var result =
            await _commentService
                .CreateReplyAsync(
                    commentId,
                    dto,
                    userId
                );

        return StatusCode(
            201,
            ApiResponse<CommentDto>
                .Success(
                    result,
                    201,
                    "Yanıt oluşturuldu."
                )
        );
    }
}