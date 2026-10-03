using BlogApi.Dtos.Auth;

namespace BlogApi.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(
        RegisterDto dto
    );

    Task<AuthResultDto> LoginAsync(
        LoginDto dto
    );

    Task<AuthResultDto> RefreshAsync(
        string refreshToken
    );

    Task LogoutAsync(
        string refreshToken
    );
}