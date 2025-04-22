using OneOf;

public interface IAuthService
{
    public Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto registerDto);

    public Task<LoginResponseDto> LoginAsync(LoginRequestDto loginDto);

    public Task RefreshTokensAsync(Guid userId, string refreshToken);
    public Task RevokeRefreshTokenAsync(Guid userId, string refreshToken);
    public Task<bool> VerifyOtpAsync(Guid userId, string otpCode);

    public Task<LoginResponseDto> CompleteProfileSetupAsync(
        Guid userId,
        CompleteProfileSetupRequestDto profileSetupDto
    );
}
