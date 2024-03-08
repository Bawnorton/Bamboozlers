using System.ComponentModel.DataAnnotations;
using Bamboozlers.Account;
using Bamboozlers.Classes.AppDbContext;
using Blazorise;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Components.Settings;

public partial class CompDeleteAccount : TabToggle
{
    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = new();
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        ViewButtonText = "Delete Account";
        ViewButtonColor = Color.Danger;
    }

    private async Task OnValidSubmitAsync()
    {
        await DataChangeCallback.InvokeAsync(new UserModel{Type=DataChangeType.Deletion, Password=Input.Password});
    }

    private sealed class InputModel
    {
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}