using Bamboozlers.Classes.AppDbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Bamboozlers.Account;

public sealed class EmailSender(
    IOptions<AuthMessageSenderOptions> optionsAccessor,
    ILogger<EmailSender> logger)
    : IEmailSender<User>
{
    private readonly ILogger _logger = logger;

    private AuthMessageSenderOptions Options { get; } = optionsAccessor.Value; //Set with Secret Manager.

    public Task SendConfirmationLinkAsync(User user, string email, string confirmationLink)
    {
        return SendEmailAsync(email, "Confirm your email",
            $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");
    }

    public Task SendPasswordResetLinkAsync(User user, string email, string resetLink)
    {
        return SendEmailAsync(email, "Reset your password",
            $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");
    }

    public Task SendPasswordResetCodeAsync(User user, string email, string resetCode)
    {
        return SendEmailAsync(email, "Reset your password",
            $"Please reset your password using the following code: {resetCode}");
    }

    private async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        if (string.IsNullOrEmpty(Options.SendGridKey)) throw new Exception("Null SendGridKey");
        await Execute(Options.SendGridKey, subject, message, toEmail);
    }

    private async Task Execute(string apiKey, string subject, string message, string toEmail)
    {
        var client = new SendGridClient(apiKey);
        var msg = new SendGridMessage
        {
            From = new EmailAddress("bamboozlersdonotreply@gmail.com", "Email Confirmation"),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message
        };
        msg.AddTo(new EmailAddress(toEmail));

        // Disable click tracking.
        // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
        msg.SetClickTracking(false, false);
        var response = await client.SendEmailAsync(msg);
        _logger.LogInformation(response.IsSuccessStatusCode
            ? $"Email to {toEmail} queued successfully!"
            : $"Failure Email to {toEmail}");
    }
}