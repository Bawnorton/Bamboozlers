using System.ComponentModel.DataAnnotations;
using Bamboozlers.Account;
using Blazorise;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Components.Settings;

public partial class CompEditDisplayName : CompTabToggle
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

        User.DisplayName = Input.DisplayName;
        
        await OnUserUpdateAsync();
        Logger.LogInformation("User changed their display name successfully.");

        await StatusCallback.InvokeAsync(new StatusCallbackArgs(
            statusColor: Color.Primary,
            statusVisible: true,
            statusMessage: "Success!",
            statusDescription: "Your display name has been changed."
            ));
    }
    
    private sealed class InputModel
    {
        [DataType(DataType.Text)]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = "";
    }
}