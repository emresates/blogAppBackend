using BlogApi.Dtos.Auth;
using BlogApi.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BlogApi.Models.Responses;

namespace BlogApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterDto dto
    )
    {
        var result = await _authService.RegisterAsync(dto);

        return StatusCode(
            201,
            ApiResponse<AuthResponseDto>.Success(
                result,
                201,
                "Kullanıcı başarıyla oluşturuldu."
            )
        );
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);

        return Ok(
            ApiResponse<AuthResponseDto>.Success(
                result,
                200,
                "Giriş başarılı."
            )
        );
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(
        ClaimTypes.NameIdentifier
    );

        var name = User.FindFirstValue(
            ClaimTypes.Name
        );

        var email = User.FindFirstValue(
            ClaimTypes.Email
        );

        var data = new
        {
            userId,
            name,
            email
        };

        return Ok(
            ApiResponse<object>.Success(
                data,
                200,
                "Kullanıcı bilgileri getirildi."
            )
        );
    }
}