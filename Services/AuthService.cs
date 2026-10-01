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
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == dto.Email);

        if (existingUser != null)
        {
            throw new AppException(
                "Bu email zaten kullanılıyor.",
                400,
                "emailAlreadyExists"
            );
        }

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email
        };

        var passwordHasher = new PasswordHasher<User>();

        user.PasswordHash = passwordHasher.HashPassword(
            user,
            dto.Password
        );

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        var accessToken = _tokenService.CreateToken(user);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == dto.Email);

        if (user == null)
        {
            throw new AppException(
                "Email veya şifre hatalı.",
                401,
                "WrongUsernameOrPassword"
            );
        }

        var passwordHasher = new PasswordHasher<User>();

        var passwordResult = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.Password
        );

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            throw new AppException(
                "Email veya şifre hatalı.",
                401,
                "WrongUsernameOrPassword"
            );
        }

        var accessToken = _tokenService.CreateToken(user);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
        };
    }
}