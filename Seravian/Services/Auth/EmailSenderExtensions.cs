public static class EmailSenderExtensions
{
    public static async Task<bool> SendOtpEmailAsync(
        this IEmailSender emailSender,
        string to,
        string otpCode,
        int expirationMinutes
    )
    {
        var subject = "Activate Your Seravian Account with OTP";
        string htmlBody =
            $@"
            <html>
            <head>
                <style>
                    body {{
                        font-family: 'Arial', sans-serif;
                        background-color: #f4f4f4;
                        text-align: center;
                        padding: 40px;
                    }}
                    .container {{
                        background: white;
                        max-width: 500px;
                        padding: 20px;
                        margin: auto;
                        border-radius: 10px;
                        box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.1);
                    }}
                    h2 {{
                        color: #4CAF50;
                    }}
                    .otp {{
                        font-size: 22px;
                        font-weight: bold;
                        color: #333;
                        padding: 10px;
                        border: 2px dashed #4CAF50;
                        display: inline-block;
                        margin: 20px 0;
                    }}
                    .footer {{
                        font-size: 12px;
                        color: #777;
                        margin-top: 20px;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <h2>Seravian OTP Verification</h2>
                    <p>Use the OTP below to verify your email:</p>
                    <div class='otp'>{otpCode}</div>
                    <p>This OTP is valid for <strong>{expirationMinutes} minutes</strong>. Do not share it with anyone.</p>
                    <p>If you didn't request this, please ignore this email.</p>
                    <div class='footer'>© {DateTime.UtcNow.Year} Seravian. All rights reserved.</div>
                </div>
            </body>
            </html>";

        return await emailSender.SendEmailAsync(to, subject, htmlBody);
    }

    public static async Task<bool> SendWelcomeEmailAsync(
        this IEmailSender emailSender,
        string toEmail,
        string fullName
    )
    {
        string subject = "Welcome to Seravian!";

        // Welcome email body
        string htmlBody =
            $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Welcome to Seravian</title>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            background-color: #f9f9f9;
                            margin: 0;
                            padding: 0;
                        }}
                        .container {{
                            width: 100%;
                            max-width: 600px;
                            margin: 0 auto;
                            background-color: #ffffff;
                            padding: 20px;
                            border-radius: 8px;
                            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                        }}
                        .header {{
                            font-size: 26px;
                            font-weight: bold;
                            color: #4CAF50;
                            text-align: center;
                            margin-bottom: 20px;
                        }}
                        .content {{
                            font-size: 16px;
                            color: #555555;
                            margin-bottom: 30px;
                        }}
                        .footer {{
                            font-size: 14px;
                            color: #888888;
                            text-align: center;
                            margin-top: 20px;
                        }}
                        .footer a {{
                            color: #4CAF50;
                            text-decoration: none;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            Welcome to Seravian, {fullName}!
                        </div>
                        <div class='content'>
                            Hello {fullName},<br><br>
                            Thank you for registering with Seravian! We're excited to have you on board. Your registration has been successfully completed.<br><br>
                            If you have any questions, feel free to contact our support team. We're here to help you!<br><br>
                            Cheers,<br>
                            The Seravian Team
                        </div>
                        <div class='footer'>
                            <p>If you didn't create an account, please ignore this email.</p>
                            <p>For support, contact us at <a href='mailto:seravian.auth@gmail.com'>seravian.auth@gmail.com</a>.</p>
                            <p>© {DateTime.UtcNow.Year} Seravian. All Rights Reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

        return await emailSender.SendEmailAsync(toEmail, subject, htmlBody);
    }
}
