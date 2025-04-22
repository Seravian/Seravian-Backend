public interface IOtpService
{
    Task GenerateAndSendOtpAsync(User user);
    Task<bool> ValidateOtpAsync(Guid userId, string code);
}
