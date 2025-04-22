using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public class TokenService : ITokenService
{
    private readonly IOptions<JwtSettings> _jwtSettings;
    private readonly ApplicationDbContext _dbContext;

    public TokenService(ApplicationDbContext dbContext, IOptions<JwtSettings> jwtSettings)
    {
        _dbContext = dbContext;
        _jwtSettings = jwtSettings;
    }

    // Method to generate both access and refresh tokens
    public async Task<AuthTokens> GenerateTokensAsync(User user)
    {
        // Generate the access token (JWT)
        var accessToken = GenerateAccessToken(user);

        // Generate a secure refresh token
        var refreshToken = GenerateSecureRefreshToken();
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtSettings.Value.RefreshTokenExpirationDays),
            IsRevoked = false,
        };
        await _dbContext.RefreshTokens.AddAsync(refreshTokenEntity);
        await _dbContext.SaveChangesAsync();

        return new AuthTokens
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenEntity.Token,
            AccessTokenExpirationUtc = DateTime.UtcNow.AddMinutes(
                _jwtSettings.Value.AccessTokenExpirationMinutes
            ),
        };
    }

    public async Task RevokeRefreshTokenAsync(Guid userId, string refreshToken)
    {
        var token = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt =>
            rt.Token == refreshToken && !rt.IsRevoked && rt.UserId == userId
        );
        if (token == null)
        {
            throw new Exception("Invalid or already revoked refresh token");
        }

        token.IsRevoked = true;
        await _dbContext.SaveChangesAsync();
    }

    // Get Refresh Token by Token String
    public async Task<RefreshToken?> GetRefreshTokenAsync(Guid userId, string token)
    {
        return await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt =>
            rt.Token == token && !rt.IsRevoked && rt.UserId == userId
        );
    }

    // Method to generate the access token (JWT)
    private string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        };

        // Fetch the secret key from the app settings
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Value.AccessTokenKey)
        );
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Create a new JWT token
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Value.Issuer,
            audience: _jwtSettings.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.Value.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );
        // Return the token as a string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateSecureRefreshToken()
    {
        var randomBytes = new byte[32];

        // Use the recommended RandomNumberGenerator.Create() pattern
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes);
    }

    public AuthTokens GenerateAccessTokenWithoutRole(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        };

        // Fetch the secret key from the app settings
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Value.AccessTokenKey)
        );
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Create a new JWT token
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Value.Issuer,
            audience: _jwtSettings.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.Value.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );
        // Return the token as a string
        return new AuthTokens
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            AccessTokenExpirationUtc = DateTime.UtcNow.AddMinutes(
                _jwtSettings.Value.AccessTokenExpirationMinutes
            ),
            RefreshToken = null,
        };
    }
}
