using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Bamboozlers.Classes.AppDbContext;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace Bamboozlers.Components.Settings;

public partial class CompEditEmail : CompTabToggle
{
    [SupplyParameterFromForm(FormName = "change-email")]
    private InputModel Input { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (User is null) return;
        ChangeViewContext();
        ViewButtonText = "Change Email";
    }

    private async Task OnValidSubmitAsync()
    {
        if (User is null)
        {
            await OnStatusUpdate(StatusCallbackArgs.BasicStatusArgs);
            return;
        }
        
        if (Input.NewEmail is null || Input.NewEmail == User?.Email)
        {
            await OnStatusUpdate(new StatusCallbackArgs(
                statusColor: Color.Danger,
                statusVisible: true,
                statusMessage: "Could not change your email.",
                statusDescription: "A different, valid email must be entered to change email."
                ));
            return;
        }

        var userId = await UserManager.GetUserIdAsync(User);
        var code = await UserManager.GenerateChangeEmailTokenAsync(User, Input.NewEmail);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = NavigationManager.GetUriWithQueryParameters(
            NavigationManager.ToAbsoluteUri("Account/ConfirmEmailChange").AbsoluteUri,
            new Dictionary<string, object?> { ["userId"] = userId, ["email"] = Input.NewEmail, ["code"] = code });

        await EmailSender.SendConfirmationLinkAsync(User, Input.NewEmail, HtmlEncoder.Default.Encode(callbackUrl));

        await OnStatusUpdate(new StatusCallbackArgs(
            statusColor: Color.Secondary,
            statusVisible: true,
            statusMessage: "Confirmation link was sent to new email.",
            statusDescription: "Please check your inbox to confirm changes."
            ));
    }

    private async Task OnSendEmailVerificationAsync()
    {
        if (User?.Email is null) return;

        var userId = await UserManager.GetUserIdAsync(User);
        var code = await UserManager.GenerateEmailConfirmationTokenAsync(User);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = NavigationManager.GetUriWithQueryParameters(
            NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
            new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code });

        await EmailSender.SendConfirmationLinkAsync(User, User.Email, HtmlEncoder.Default.Encode(callbackUrl));

        await OnStatusUpdate(new StatusCallbackArgs(
            statusColor: Color.Secondary,
            statusVisible: true,
            statusMessage: "Confirmation link was sent to your new email.",
            statusDescription: "Please check your inbox to confirm changes."
            ));
    }

    private sealed class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "New email")]
        public string? NewEmail { get; set; }
    }
}