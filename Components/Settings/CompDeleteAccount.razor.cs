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
            await StatusCallback.InvokeAsync(StatusCallbackArgs.BasicStatusArgs);
            return;
        }
        
        if (HasPassword && !await UserManager.CheckPasswordAsync(User, Input.Password))
        {
            await StatusCallback.InvokeAsync(new StatusCallbackArgs(
                Color.Danger,
                true,
                "Error occurred while deleting account.",
                "Incorrect password."
            ));
            return;
        }

        var result = await UserManager.DeleteAsync(User);
        if (!result.Succeeded)
        {
            await StatusCallback.InvokeAsync(new StatusCallbackArgs(
                Color.Danger,
                true,
                "Error occurred while deleting account.",
                "Unexpected error occurred while deleting your account."
            ));
            throw new InvalidOperationException("Unexpected error occurred deleting user.");
        }

        await SignInManager.SignOutAsync();

        var userId = await UserManager.GetUserIdAsync(User);
        Logger.LogInformation("User with ID '{UserId}' deleted their account.", userId);

        NavigationManager.NavigateTo("/Account/Login");
    }

    private sealed class InputModel
    {
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}