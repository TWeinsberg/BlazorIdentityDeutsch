using BlazorIdentityDeutsch.Data;
using BlazorIdentityDeutsch.Models;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace BlazorIdentityDeutsch.Services {
    public sealed class IdentityEmailSender(IConfiguration configuration, IStringLocalizer<IdentityTexts> T) : IEmailSender<ApplicationUser> {
        public Task SendConfirmationLinkAsync(
            ApplicationUser user,
            string email,
            string confirmationLink) {
            return SendAsync(
                email,
                T["Confirm your email"],
                $"""
            <p>{T["Confirm your email"]}.</p>
            <p><a href="{confirmationLink}">{T["Confirm email"]}</a></p>
            """);
        }

        public Task SendPasswordResetLinkAsync(
            ApplicationUser user,
            string email,
            string resetLink) {
            return SendAsync(
                email,
                T["Reset your password"],
                $"""
            <p> {T["You have requested a password reset"]}.</p>
            <p><a href="{resetLink}">
                {T["Reset your password"]}
            </a></p>
            """);
        }

        public Task SendPasswordResetCodeAsync(
            ApplicationUser user,
            string email,
            string resetCode) {
            return SendAsync(
                email,
                T["Reset password"],
                $"""
            <p>{T["Your password reset code is"]}: {resetCode}</p>
            <p><strong>{resetCode}</strong></p>
            """);
        }

        private async Task SendAsync(
            string email,
            string subject,
            string html) {
            var settings = configuration
                .GetSection("Smtp")
                .Get<SmtpSettings>()
                ?? throw new InvalidOperationException("SMTP configuration missing.");

            var message = new MimeKit.MimeMessage();

            message.From.Add(
                new MimeKit.MailboxAddress(
                    settings.SenderName,
                    settings.SenderEmail));

            message.To.Add(
                MimeKit.MailboxAddress.Parse(email));

            message.Subject = subject;

            message.Body = new MimeKit.BodyBuilder {
                HtmlBody = html
            }.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();

            await client.ConnectAsync(
                settings.Host,
                settings.Port,
                settings.SslOptions ?? SecureSocketOptions.None);

            await client.AuthenticateAsync(
                settings.UserName,
                settings.Password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
