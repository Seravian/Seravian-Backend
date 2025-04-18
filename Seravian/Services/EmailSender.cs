using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

public class EmailSender : IEmailSender
{
    private readonly IOptions<EmailSettings> _emailSettings;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<EmailSettings> emailSettings, ILogger<EmailSender> logger)
    {
        _logger = logger;
        _emailSettings = emailSettings;
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(
            new MailboxAddress(_emailSettings.Value.SenderName, _emailSettings.Value.SenderEmail)
        );
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = body };
        message.Body = bodyBuilder.ToMessageBody();

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(
                _emailSettings.Value.SmtpServer,
                _emailSettings.Value.Port,
                SecureSocketOptions.StartTls
            );
            await client.AuthenticateAsync(
                _emailSettings.Value.SenderEmail,
                _emailSettings.Value.AppPassword
            );
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        try
        {
            // Create the SMTP client and send the email
            using (var client = new SmtpClient())
            {
                // Connect to the SMTP server using StartTLS for security
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

                // Authenticate with Gmail using app-specific password
                await client.AuthenticateAsync(
                    _emailSettings.Value.SenderEmail,
                    _emailSettings.Value.AppPassword
                );

                // Send the email message
                await client.SendAsync(message);

                // Disconnect after sending the message
                await client.DisconnectAsync(true);
            }
            return true; // Indicate success
        }
        catch (SmtpCommandException smtpEx)
        {
            _logger.LogError(smtpEx, "SMTP command error occurred while sending email.");
            return false; // Indicate failure
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while sending email.");
            return false; // Indicate failure
        }
    }
}
