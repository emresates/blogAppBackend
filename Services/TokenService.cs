using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BlogApi.Models;
using Microsoft.IdentityModel.Tokens;

namespace BlogApi.Services;

public class TokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateRefreshToken()
    {
        var randomBytes =
            RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(
            randomBytes
        );
    }

    public string HashRefreshToken(
    string token
    )
    {
        var bytes =
            Encoding.UTF8.GetBytes(token);

        var hash =
            SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }

    public string CreateAccessToken(User user)
    {
        var jwtKey =
            _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "Jwt:Key bulunamadı."
            );

        var issuer =
            _configuration["Jwt:Issuer"];

        var audience =
            _configuration["Jwt:Audience"];

        var claims = new List<Claim>
    {
        new(
            ClaimTypes.NameIdentifier,
            user.Id.ToString()
        ),

        new(
            ClaimTypes.Name,
            user.Name
        ),

        new(
            ClaimTypes.Email,
            user.Email
        ),

        new(
            ClaimTypes.Role,
            user.Role
        )
    };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,

            // ACCESS TOKEN KISA ÖMÜRLÜ
            expires: DateTime.UtcNow.AddMinutes(15),

            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}