using System.ComponentModel.DataAnnotations;
using Bamboozlers.Account;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Bamboozlers.Components.Settings;

public partial class CompEditUsername : CompTabToggle
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
            await StatusCallback.InvokeAsync(new StatusCallbackArgs(
                statusColor: Color.Danger,
                statusVisible: true,
                statusMessage: "Could not change your username.",
                statusDescription: "Your user data could not be verified at this time."
                ));
            return;
        }

        var passwordResult = await UserManager.CheckPasswordAsync(User, Input.Password);

        if (passwordResult == false)
        {
            await StatusCallback.InvokeAsync(new StatusCallbackArgs(
                statusColor: Color.Danger,
                statusVisible: true,
                statusMessage: "Error occurred while changing username.",
                statusDescription: "Password entered was incorrect."
            ));
            return;
        }

        var changeUsernameResult = await UserManager.SetUserNameAsync(User, Input.Username);
        if (!changeUsernameResult.Succeeded)
        {
            await StatusCallback.InvokeAsync(new StatusCallbackArgs(
                statusColor: Color.Danger,
                statusVisible: true,
                statusMessage: "Error occurred while changing username.",
                statusDescription: $"Error: {string.Join(",", changeUsernameResult.Errors.Select(error => error.Description))}"
            ));
            return;
        }

        await OnUserUpdateAsync();
        Logger.LogInformation("User changed their username successfully.");

        await StatusCallback.InvokeAsync(new StatusCallbackArgs(
            statusColor: Color.Primary,
            statusVisible: true,
            statusMessage: "Success!",
            statusDescription: "Your username has been changed."
            ));
    }
    
    private sealed class InputModel
    {
        [Required]
        [ValidUsername(ErrorMessage = "Username is invalid. It can only contain letters, numbers, and underscores. There can only be 1 underscore in a row.")]
        [Display(Name = "Username")]
        public string Username { get; set; } = "";
        
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = "";
    }
}