using BlazorIdentityDeutsch.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Localization;

namespace BlazorIdentityDeutsch.Components.Account;

// Remove the "else if (EmailSender is IdentityNoOpEmailSender)" block from RegisterConfirmation.razor after updating with a real implementation.
internal sealed class IdentityNoOpEmailSender(IStringLocalizer<IdentityTexts> T) : IEmailSender<ApplicationUser>
{
    private readonly IEmailSender emailSender = new NoOpEmailSender();

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        emailSender.SendEmailAsync(email, T["Confirm your email"],
            T["Please confirm your account by <a href='{0}'>clicking here</a>.", confirmationLink]);

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        emailSender.SendEmailAsync(email, T["Reset your password"],
            T["Please reset your password by <a href='{0}'>clicking here</a>.", resetLink]);

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        emailSender.SendEmailAsync(email, T["Reset your password"],
            T["Please reset your password using the following code: {0}", resetCode]);
}
