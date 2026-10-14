namespace BlazorIdentityDeutsch.Services {
    public sealed class IdentityEmailSender(IConfiguration configuration, IWebHostEnvironment environment, IStringLocalizer<IdentityTexts> T ) : IEmailSender<ApplicationUser> {

        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) {
            string subject = T["Confirm your email"];

            string html = await CreateHtmlAsync(
                subject,
                T["Please confirm your email address"],
                confirmationLink,
                T["Confirm email"]);

            await SendAsync(email, subject, html);
        }

        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) {
            string subject = T["Reset your password"];

            string html = await CreateHtmlAsync(
                subject,
                T["You have requested a password reset"],
                resetLink,
                T["Reset your password"]);

            await SendAsync(email, subject, html);
        }

        public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) {
            string subject = T["Reset password"];

            string html = await CreateHtmlAsync(
                subject,
                T["Your password reset code is"],
                code: resetCode);

            await SendAsync(email, subject, html);
        }


        private async Task SendAsync(string email, string subject, string html) {
            var settings = configuration
                .GetSection("Smtp")
                .Get<SmtpSettings>()
                ?? throw new InvalidOperationException("SMTP configuration missing.");

            if (!settings.Enabled) { return; }

            var message = new MimeKit.MimeMessage();

            message.From.Add(new MimeKit.MailboxAddress(settings.SenderName, settings.SenderEmail));

            message.To.Add(MimeKit.MailboxAddress.Parse(email));

            message.Subject = subject;

            message.Body = new MimeKit.BodyBuilder {
                HtmlBody = html
            }.ToMessageBody();

            string logoFileName = Path.Combine(environment.ContentRootPath, "EmailTemplates", "logo.png");
            if (File.Exists(logoFileName)) {
                var builder = new MimeKit.BodyBuilder();
                var logo = builder.LinkedResources.Add(logoFileName);
                logo.ContentId = "application-logo";
                builder.HtmlBody = html.Replace("{{LogoUrl}}", "cid:application-logo");
                message.Body = builder.ToMessageBody();
            }

            using var client = new MailKit.Net.Smtp.SmtpClient();

            await client.ConnectAsync(
                settings.Host,
                settings.Port,
                settings.SslOptions ?? SecureSocketOptions.Auto);

            if (!string.IsNullOrWhiteSpace(settings.UserName) && !string.IsNullOrWhiteSpace(settings.Password)) {
                await client.AuthenticateAsync(settings.UserName, settings.Password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task<string> CreateHtmlAsync(string title, string message, string? actionUrl = null, string? actionText = null, string? code = null) {
            string path = Path.Combine(environment.ContentRootPath, "EmailTemplates", "identity-email-template.html");

            string html = await File.ReadAllTextAsync(path);

            string actionBlock = string.Empty;

            if (!string.IsNullOrWhiteSpace(actionUrl)) {
                actionUrl = System.Net.WebUtility.HtmlDecode(actionUrl);
                actionBlock = $"""
            <p style="text-align:center;margin:30px 0;">
                <a href="{System.Net.WebUtility.HtmlEncode(actionUrl)}"
                   style="background:#2366a6;color:#ffffff;
                          padding:14px 28px;text-decoration:none;
                          border-radius:6px;display:inline-block;">
                    {System.Net.WebUtility.HtmlEncode(actionText ?? title)}
                </a>
            </p>
            """;
            } else if (!string.IsNullOrWhiteSpace(code)) {
                code = System.Net.WebUtility.HtmlDecode(code);
                actionBlock = $"""
            <div style="text-align:center;margin:30px 0;">
                <span style="font-size:32px;font-weight:bold;
                             letter-spacing:6px;color:#163c68;">
                    {System.Net.WebUtility.HtmlEncode(code)}
                </span>
            </div>
            """;
            }

            string securityNotice = T["If you did not request this action, you can ignore this email."];
            string? preheader = null;

            return html
                .Replace("{{AppName}}", "PROJECT_DISPLAY_NAME")
                .Replace("{{Title}}", System.Net.WebUtility.HtmlEncode(title))
                .Replace("{{UserName}}", "")
                .Replace("{{Message}}", System.Net.WebUtility.HtmlEncode(message))
                .Replace("{{ActionBlock}}", actionBlock)
                .Replace("{{Year}}", DateTime.Now.Year.ToString())
                .Replace("{{SecurityNotice}}", System.Net.WebUtility.HtmlEncode(securityNotice ?? ""))
                .Replace("{{Preheader}}", System.Net.WebUtility.HtmlEncode(preheader ?? message));
        }
    }
}
