namespace backend.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendConfirmationEmailAsync(string to, string code);
    Task SendPasswordResetEmailAsync(string to, string resetLink);
}
