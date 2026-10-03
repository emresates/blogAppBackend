using BlogApi.Constants;
using BlogApi.Data;
using BlogApi.Dtos.Auth;
using BlogApi.Exceptions;
using BlogApi.Interfaces;
using BlogApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;

    public AuthService(
        AppDbContext context,
        TokenService tokenService
    )
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(
        RegisterDto dto
    )
    {
        var email = dto.Email
            .Trim()
            .ToLowerInvariant();

        var existingUser = await _context.Users
            .AnyAsync(x => x.Email == email);

        if (existingUser)
        {
            throw new AppException(
                "Bu email zaten kullanılıyor.",
                400,
                "emailAlreadyExists"
            );
        }

        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = email,
            Role = Roles.User,
            IsActive = true
        };

        var passwordHasher =
            new PasswordHasher<User>();

        user.PasswordHash =
            passwordHasher.HashPassword(
                user,
                dto.Password
            );

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        var accessToken =
            _tokenService.CreateToken(user);

        return new AuthResponseDto
        {
            AccessToken = accessToken
        };
    }

    public async Task<AuthResponseDto> LoginAsync(
        LoginDto dto
    )
    {
        var email = dto.Email
            .Trim()
            .ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(
                x => x.Email == email
            );

        if (user == null)
        {
            throw new AppException(
                "Email veya şifre hatalı.",
                401,
                "invalidCredentials"
            );
        }

        if (!user.IsActive)
        {
            throw new AppException(
                "Hesabınız devre dışı bırakılmış.",
                403,
                "userDisabled"
            );
        }

        var passwordHasher =
            new PasswordHasher<User>();

        var result =
            passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.Password
            );

        if (result ==
            PasswordVerificationResult.Failed)
        {
            throw new AppException(
                "Email veya şifre hatalı.",
                401,
                "invalidCredentials"
            );
        }

        var accessToken =
            _tokenService.CreateToken(user);

        return new AuthResponseDto
        {
            AccessToken = accessToken
        };
    }
}