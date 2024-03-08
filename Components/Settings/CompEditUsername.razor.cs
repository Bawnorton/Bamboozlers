using System.ComponentModel.DataAnnotations;
using Bamboozlers.Account;
using Bamboozlers.Classes.Services;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Bamboozlers.Components.Settings;

public partial class CompEditUsername : TabToggle
{
    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        ViewButtonText = "Change Username";
    }
    private async Task OnValidSubmitAsync()
    {
        var model = new UserModel
        {
            Type = DataChangeType.Username,
            UserName = Input.Username,
            Password = Input.Password
        };
        await DataChangeCallback.InvokeAsync(model);
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