public interface IOtpService
{
    Task GenerateAndSendOtpAsync(User user);
    Task<bool> ValidateOtpAsync(string userEmail, string code);
}
