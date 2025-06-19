using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class OtpService : IOtpService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly EmailSettings _emailSettings;
    private readonly OtpSettings _otpSettings;

    public OtpService(
        ApplicationDbContext context,
        IEmailSender emailSender,
        IOptions<EmailSettings> emailOptions,
        IOptions<OtpSettings> otpOptions
    )
    {
        _context = context;
        _emailSender = emailSender;
        _emailSettings = emailOptions.Value;
        _otpSettings = otpOptions.Value;
    }

    public async Task GenerateAndSendOtpAsync(User user)
    {
        if (user.IsEmailVerified)
            throw new Exception("Email already verified.");

        var existingOtp = await _context
            .EmailVerificationOtpCodes.Where(o =>
                o.UserId == user.Id && !o.IsConsumed && o.ExpiresAtUtc > DateTime.UtcNow
            )
            .OrderByDescending(o => o.ExpiresAtUtc)
            .FirstOrDefaultAsync();

        if (existingOtp is not null)
        {
            var remainingMinutes = (int)(existingOtp.ExpiresAtUtc - DateTime.UtcNow).TotalMinutes;
            if (remainingMinutes > _otpSettings.MinimumRemainingMinutesToResend)
            {
                await _emailSender.SendOtpEmailAsync(
                    user.Email,
                    existingOtp.Code,
                    remainingMinutes
                );
                return;
            }
        }

        var newOtp = GenerateAlphanumericOtp(_otpSettings.OtpLength);
        var newOtpEntity = new EmailVerificationOtp
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Code = newOtp,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(_otpSettings.ExpiryMinutes),
            IsConsumed = false,
        };

        await _context.EmailVerificationOtpCodes.AddAsync(newOtpEntity);
        await _context.SaveChangesAsync();

        // Send OTP email using the email extension
        await _emailSender.SendOtpEmailAsync(user.Email, newOtp, _otpSettings.ExpiryMinutes);
    }

    public async Task<bool> ValidateOtpAsync(string userEmail, string code)
    {
        User? user = await _context
            .Users.Include(x => x.EmailVerificationOtps)
            .FirstOrDefaultAsync(u => u.Email == userEmail);
        if (user is null)
            throw new Exception("User not found.");
        if (user.IsEmailVerified)
            throw new Exception("Email already verified.");

        var otp = user
            .EmailVerificationOtps.Where(x =>
                x.Code == code && !x.IsConsumed && x.ExpiresAtUtc > DateTime.UtcNow
            )
            .OrderByDescending(x => x.ExpiresAtUtc)
            .FirstOrDefault();

        if (otp == null)
            return false;

        otp.IsConsumed = true;
        user.IsEmailVerified = true;
        user.EmailVerifiedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    private static string GenerateAlphanumericOtp(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var byteArray = new byte[length];

        // Fill the byte array with secure random bytes
        RandomNumberGenerator.Fill(byteArray);

        // Map each byte to a character in the charset
        return new string(byteArray.Select(b => chars[b % chars.Length]).ToArray());
    }
}
