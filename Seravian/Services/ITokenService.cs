public interface ITokenService
{
    Task<AuthTokens> GenerateTokensAsync(User user);
    Task RevokeRefreshTokenAsync(Guid userId, string refreshToken);
    Task<RefreshToken?> GetRefreshTokenAsync(Guid userId, string token);
    AuthTokens GenerateAccessTokenWithoutRole(User user);
}
