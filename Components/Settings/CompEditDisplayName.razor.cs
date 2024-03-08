using System.ComponentModel.DataAnnotations;
using Bamboozlers.Account;
using Blazorise;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Components.Settings;

public partial class CompEditDisplayName : TabToggle
{
    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        ViewButtonText = "Change Password";
    }
    
    private async Task OnValidSubmitAsync()
    {
        var model = new UserModel
        {
            Type = DataChangeType.Visual,
            DisplayName = Input.DisplayName
        };
        await DataChangeCallback.InvokeAsync(model);
        
        Logger.LogInformation("User changed their display name successfully.");

        UpdateStatusArgs(new StatusArguments(
            statusColor: Color.Primary,
            statusVisible: true,
            statusMessage: "Success!",
            statusDescription: "Your display name has been changed."
            ));

        await Toggle();
    }
    
    private sealed class InputModel
    {
        [DataType(DataType.Text)]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = "";
    }
}