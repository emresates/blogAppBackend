using BlogApi.Dtos.Posts;
using BlogApi.Interfaces;
using BlogApi.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogApi.Controllers;

[ApiController]
[Route("api/posts")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var posts = await _postService.GetAllAsync();

        return Ok(
            ApiResponse<List<PostDto>>.Success(
                posts,
                200,
                "Postlar getirildi."
            )
        );
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var post = await _postService.GetByIdAsync(id);

        return Ok(
            ApiResponse<PostDto>.Success(
                post,
                200,
                "Post getirildi."
            )
        );
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(
        CreatePostDto dto
    )
    {
        var userId = GetCurrentUserId();

        var post = await _postService.CreateAsync(
            dto,
            userId
        );

        return StatusCode(
            201,
            ApiResponse<PostDto>.Success(
                post,
                201,
                "Post oluşturuldu."
            )
        );
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        UpdatePostDto dto
    )
    {
        var userId = GetCurrentUserId();

        var post = await _postService.UpdateAsync(
            id,
            dto,
            userId
        );

        return Ok(
            ApiResponse<PostDto>.Success(
                post,
                200,
                "Post güncellendi."
            )
        );
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();

        await _postService.DeleteAsync(
            id,
            userId
        );

        return Ok(
            ApiResponse<object>.Success(
                new { },
                200,
                "Post silindi."
            )
        );
    }

    private int GetCurrentUserId()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        return int.Parse(userId!);
    }
}