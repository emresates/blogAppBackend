using BlogApi.Dtos.Auth;
using BlogApi.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BlogApi.Models.Responses;
using BlogApi.Exceptions;

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
        var result =
    await _authService.RegisterAsync(dto);

        SetRefreshTokenCookie(
            result.RefreshToken
        );

        var response = new AuthResponseDto
        {
            AccessToken =
                result.AccessToken
        };

        return StatusCode(
            201,
            ApiResponse<AuthResponseDto>.Success(
                response,
                201,
                "Kullanıcı başarıyla oluşturuldu."
            )
        );
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
    LoginDto dto
)
    {
        var result =
            await _authService.LoginAsync(dto);

        SetRefreshTokenCookie(
            result.RefreshToken
        );

        var response =
            new AuthResponseDto
            {
                AccessToken =
                    result.AccessToken
            };

        return Ok(
            ApiResponse<AuthResponseDto>.Success(
                response,
                200,
                "Giriş başarılı."
            )
        );
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        var name =
            User.FindFirstValue(
                ClaimTypes.Name
            );

        var email =
            User.FindFirstValue(
                ClaimTypes.Email
            );

        var role =
            User.FindFirstValue(
                ClaimTypes.Role
            );

        var data = new
        {
            userId,
            name,
            email,
            role
        };

        return Ok(
            ApiResponse<object>.Success(
                data,
                200,
                "Kullanıcı bilgileri getirildi."
            )
        );
    }

    private void SetRefreshTokenCookie(
    string refreshToken
)
    {
        Response.Cookies.Append(
            "refreshToken",
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true,

                Secure = true,

                SameSite = SameSiteMode.None,

                Expires =
                    DateTimeOffset.UtcNow.AddDays(7)
            }
        );
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        if (
            !Request.Cookies.TryGetValue(
                "refreshToken",
                out var refreshToken
            )
            ||
            string.IsNullOrWhiteSpace(
                refreshToken
            )
        )
        {
            throw new AppException(
                "Refresh token bulunamadı.",
                401,
                "refreshTokenMissing"
            );
        }

        var result =
            await _authService.RefreshAsync(
                refreshToken
            );

        SetRefreshTokenCookie(
            result.RefreshToken
        );

        var response =
            new AuthResponseDto
            {
                AccessToken =
                    result.AccessToken
            };

        return Ok(
            ApiResponse<AuthResponseDto>.Success(
                response,
                200,
                "Token yenilendi."
            )
        );
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (
            Request.Cookies.TryGetValue(
                "refreshToken",
                out var refreshToken
            )
            &&
            !string.IsNullOrWhiteSpace(
                refreshToken
            )
        )
        {
            await _authService.LogoutAsync(
                refreshToken
            );
        }

        Response.Cookies.Delete(
            "refreshToken",
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite =
                    SameSiteMode.None
            }
        );

        return Ok(
            ApiResponse<object>.Success(
                new { },
                200,
                "Çıkış yapıldı."
            )
        );
    }
}