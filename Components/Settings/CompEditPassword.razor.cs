using System.ComponentModel.DataAnnotations;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

namespace Bamboozlers.Components.Settings;

public partial class CompEditPassword : CompTabToggle
{
    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (User == null) return;
        ViewButtonText = "Change Password";
    }
    private async Task OnValidSubmitAsync()
    {
        if (User == null)
        {
            await OnStatusUpdate(new StatusCallbackArgs(
                statusColor: Color.Danger,
                statusVisible: true,
                statusMessage: "Could not change your password.",
                statusDescription: "Your user data could not be verified at this time."
                ));
            return;
        }
        var changePasswordResult = await UserManager.ChangePasswordAsync(User, Input.OldPassword, Input.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            await OnStatusUpdate(new StatusCallbackArgs(
                statusColor: Color.Danger,
                statusVisible: true,
                statusMessage: "Error occurred while changing password:",
                statusDescription: $"Error: {string.Join(",", changePasswordResult.Errors.Select(error => error.Description))}"
                ));
        }

        await SignInManager.RefreshSignInAsync(User);
        Logger.LogInformation("User changed their password successfully.");

        await OnStatusUpdate(new StatusCallbackArgs(
            statusColor: Color.Primary,
            statusVisible: true,
            statusMessage: "Success!",
            statusDescription: "Your password has been changed."
            ));
    }

    private sealed class InputModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; } = "";

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = "";
    }
}