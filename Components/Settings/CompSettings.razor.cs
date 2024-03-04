using System.ComponentModel.DataAnnotations;
using Bamboozlers.Account;
using Bamboozlers.Classes.AppDbContext;
using Blazorise;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Components.Settings;

public partial class CompSettings : ComponentBase
{
    [Parameter]
    public EventCallback<string> NavigatorCallback { get; set; }
    private bool PopupEnabled { get; set; } = true;
    protected User? User { get; set; }
    protected string? Email { get; set; }
    protected string? Username { get; set; }
    protected bool HasPassword { get; set; }
    protected bool IsEmailConfirmed { get; set; }

    protected StatusCallbackArgs alertStatus = new();
    private string? SectionName { get; set; } = "User Profile";

    protected override async Task OnInitializedAsync()
    { 
        await OnUserUpdateAsync();
    }

    protected async Task OnUserUpdateAsync()
    {
        User = await UserManager.GetUserAsync(UserService.GetUser());
        if (User is null)
        {
            await OnStatusUpdate(StatusCallbackArgs.BasicStatusArgs);
            return;
        }
        Email = await UserManager.GetEmailAsync(User);
        HasPassword = await UserManager.HasPasswordAsync(User);
        IsEmailConfirmed = await UserManager.IsEmailConfirmedAsync(User);
        Username = await UserManager.GetUserNameAsync(User);
    }
    
    protected Task OnStatusUpdate(StatusCallbackArgs args)
    {
        alertStatus = args;
        return Task.CompletedTask;
    }

    protected async Task LogOut()
    {
        if (User != null)
        {
            await SignInManager.SignOutAsync();
            await NavigatorCallback.InvokeAsync("/Account/Login");
        }
    }
}

public sealed class InputModel
{
    [DataType(DataType.Text)]
    [Display(Name = "Display Name")]
    public string DisplayName { get; set; } = "";
        
    [Required]
    [ValidUsername(ErrorMessage = "Username is invalid. It can only contain letters, numbers, and underscores. There can only be 1 underscore in a row.")]
    [Display(Name = "Username")]
    public string Username { get; set; } = "";
}

public sealed class StatusCallbackArgs
{
    public static readonly StatusCallbackArgs BasicStatusArgs = new (
        Color.Danger,
        true,
        "Could not find your account details.",
        "Your user data could not be verified at this time."
    );
    
    public readonly Color StatusColor;
    public readonly string StatusDescription;
    public readonly string StatusMessage;
    public readonly bool StatusVisible;

    public StatusCallbackArgs()
    {
        StatusColor = Color.Default;
        StatusVisible = false;
        StatusMessage = "";
        StatusDescription = "";
    }

    public StatusCallbackArgs(Color statusColor, bool statusVisible, string statusMessage, string statusDescription)
    {
        StatusColor = statusColor;
        StatusVisible = statusVisible;
        StatusMessage = statusMessage;
        StatusDescription = statusDescription;
    }
}