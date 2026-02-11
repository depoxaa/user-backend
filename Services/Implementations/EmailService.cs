using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using backend.Configuration;
using backend.Services.Interfaces;

namespace backend.Services.Implementations;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }

    public async Task SendConfirmationEmailAsync(string to, string code)
    {
        var subject = "Confirm your Aganim account";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; background-color: #1e293b; color: white; padding: 20px;'>
                <div style='max-width: 600px; margin: 0 auto; background-color: #0f172a; border-radius: 10px; padding: 30px;'>
                    <h1 style='color: #3b82f6; text-align: center;'>Welcome to Aganim!</h1>
                    <p style='font-size: 16px; text-align: center;'>Your email confirmation code is:</p>
                    <div style='background-color: #1e293b; padding: 20px; border-radius: 8px; text-align: center; margin: 20px 0;'>
                        <span style='font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #3b82f6;'>{code}</span>
                    </div>
                    <p style='font-size: 14px; color: #94a3b8; text-align: center;'>This code will expire in 15 minutes.</p>
                    <p style='font-size: 14px; color: #94a3b8; text-align: center;'>If you didn't create an account, please ignore this email.</p>
                </div>
            </body>
            </html>";

        await SendEmailAsync(to, subject, body);
    }

    public async Task SendPasswordResetEmailAsync(string to, string resetLink)
    {
        var subject = "Reset your Aganim password";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; background-color: #1e293b; color: white; padding: 20px;'>
                <div style='max-width: 600px; margin: 0 auto; background-color: #0f172a; border-radius: 10px; padding: 30px;'>
                    <h1 style='color: #3b82f6; text-align: center;'>Password Reset</h1>
                    <p style='font-size: 16px; text-align: center;'>Click the button below to reset your password:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{resetLink}' style='background-color: #3b82f6; color: white; padding: 15px 30px; text-decoration: none; border-radius: 8px; font-weight: bold;'>Reset Password</a>
                    </div>
                    <p style='font-size: 14px; color: #94a3b8; text-align: center;'>This link will expire in 1 hour.</p>
                    <p style='font-size: 14px; color: #94a3b8; text-align: center;'>If you didn't request a password reset, please ignore this email.</p>
                </div>
            </body>
            </html>";

        await SendEmailAsync(to, subject, body);
    }
}
