using System.ComponentModel.DataAnnotations;
using Bamboozlers.Classes.Services;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

namespace Bamboozlers.Components.Settings;

public partial class CompEditPassword : TabToggle
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
            Type = DataChangeType.Password,
            Password = Input.OldPassword,
            NewPassword = Input.NewPassword
        };
        await DataChangeCallback.InvokeAsync(model);
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