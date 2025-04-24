public interface ITokenService
{
    Task<AuthTokens> GenerateTokensAsync(User user);
    Task RevokeRefreshTokenAsync(string refreshToken);
    Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken);
    AuthTokens GenerateAccessTokenWithoutRole(User user);
}
