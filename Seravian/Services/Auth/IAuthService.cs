public interface IAuthService
{
    public Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto registerDto);
    public Task ResendOtpAsync(string email);
    public Task<LoginResponseDto> LoginAsync(LoginRequestDto loginDto);

    public Task<AuthTokens> RefreshTokensAsync(string refreshToken);
    public Task RevokeRefreshTokenAsync(string refreshToken);
    public Task<bool> VerifyOtpAsync(string email, string otpCode);

    public Task<LoginResponseDto> CompleteProfileSetupAsync(
        Guid userId,
        CompleteProfileSetupRequestDto profileSetupDto
    );
}
