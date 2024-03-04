using System.ComponentModel.DataAnnotations;
using Bamboozlers.Classes.AppDbContext;
using Blazorise;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Components.Settings;

public partial class CompDeleteAccount : CompTabToggle
{
    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (User == null) return;
        ViewButtonText = "Delete Account";
        ViewButtonColor = Color.Danger;
    }

    private async Task OnValidSubmitAsync()
    {
        if (User == null)
        {
            await OnStatusUpdate(StatusCallbackArgs.BasicStatusArgs);
            return;
        }
        
        if (HasPassword && !await UserManager.CheckPasswordAsync(User, Input.Password))
        {
            await OnStatusUpdate(new StatusCallbackArgs(
                Color.Danger,
                true,
                "Error occurred:",
                "Incorrect password."
            ));
            return;
        }

        var result = await UserManager.DeleteAsync(User);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Unexpected error occurred deleting user.");
        }

        await SignInManager.SignOutAsync();

        var userId = await UserManager.GetUserIdAsync(User);
        Logger.LogInformation("User with ID '{UserId}' deleted their account.", userId);

        RedirectManager.RedirectTo("/Account/Login");
    }

    private sealed class InputModel
    {
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}