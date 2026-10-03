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

    public async Task<AuthResultDto> RegisterAsync(
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
    _tokenService.CreateAccessToken(user);

        var refreshToken =
            await CreateRefreshTokenAsync(user);

        return new AuthResultDto
        {
            AccessToken = accessToken,

            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResultDto> LoginAsync(
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
            _tokenService.CreateAccessToken(user);

        var refreshToken =
            await CreateRefreshTokenAsync(user);

        return new AuthResultDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }


    private async Task<string> CreateRefreshTokenAsync(
    User user
)
    {
        var rawToken =
            _tokenService.CreateRefreshToken();

        var tokenHash =
            _tokenService.HashRefreshToken(
                rawToken
            );

        var refreshToken = new RefreshToken
        {
            TokenHash = tokenHash,

            UserId = user.Id,

            CreatedAt = DateTime.UtcNow,

            ExpiresAt =
                DateTime.UtcNow.AddDays(7)
        };

        _context.RefreshTokens.Add(
            refreshToken
        );

        await _context.SaveChangesAsync();

        return rawToken;
    }

    public async Task<AuthResultDto> RefreshAsync(
    string refreshToken
)
    {
        var tokenHash =
            _tokenService.HashRefreshToken(
                refreshToken
            );

        var storedToken =
            await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(
                    x => x.TokenHash == tokenHash
                );

        if (
            storedToken == null ||
            !storedToken.IsActive
        )
        {
            throw new AppException(
                "Refresh token geçersiz veya süresi dolmuş.",
                401,
                "invalidRefreshToken"
            );
        }

        if (!storedToken.User.IsActive)
        {
            throw new AppException(
                "Hesap devre dışı.",
                403,
                "userDisabled"
            );
        }

        // Eski refresh token artık kullanılamaz.
        storedToken.RevokedAt =
            DateTime.UtcNow;

        var newRawRefreshToken =
            _tokenService.CreateRefreshToken();

        var newTokenHash =
            _tokenService.HashRefreshToken(
                newRawRefreshToken
            );

        storedToken.ReplacedByTokenHash =
            newTokenHash;

        var newRefreshToken =
            new RefreshToken
            {
                TokenHash =
                    newTokenHash,

                UserId =
                    storedToken.UserId,

                CreatedAt =
                    DateTime.UtcNow,

                ExpiresAt =
                    DateTime.UtcNow.AddDays(7)
            };

        _context.RefreshTokens.Add(
            newRefreshToken
        );

        var accessToken =
            _tokenService.CreateAccessToken(
                storedToken.User
            );

        await _context.SaveChangesAsync();

        return new AuthResultDto
        {
            AccessToken =
                accessToken,

            RefreshToken =
                newRawRefreshToken
        };
    }

    public async Task LogoutAsync(
    string refreshToken
)
    {
        var tokenHash =
            _tokenService.HashRefreshToken(
                refreshToken
            );

        var storedToken =
            await _context.RefreshTokens
                .FirstOrDefaultAsync(
                    x =>
                        x.TokenHash ==
                        tokenHash
                );

        if (
            storedToken != null &&
            storedToken.IsActive
        )
        {
            storedToken.RevokedAt =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}