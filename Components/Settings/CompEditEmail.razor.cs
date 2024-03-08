using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Components.Settings;

public partial class CompEditEmail : TabToggle
{
    [SupplyParameterFromForm(FormName = "change-email")]
    private InputModel Input { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        ChangeViewContext();
        ViewButtonText = "Change Email";
    }

    private async Task OnValidSubmitAsync()
    {
        await DataChangeCallback.InvokeAsync(new UserModel { Type = DataChangeType.Email, Email = Input.NewEmail });
    }

    private sealed class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "New email")]
        public string? NewEmail { get; set; }
    }
}