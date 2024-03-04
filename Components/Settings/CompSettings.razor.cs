using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Bamboozlers.Account;
using Bamboozlers.Classes;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Services;
using Bamboozlers.Components.Settings;
using Blazorise;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Components.Settings;

public partial class CompSettings : ComponentBase
{
    private bool _visible;
    
    [Parameter]
    public bool Visible { 
        get => _visible;
        set
        {
            if (_visible == value) return;
            _visible = value;
            VisibleChanged.InvokeAsync(value);
        }
    }
    
    public StatusCallbackArgs CallbackArgs { get; set; }  = new();

    [Parameter] 
    public EventCallback<StatusCallbackArgs> StatusCallback { get; set; }
    
    public async Task OnStatusUpdate(StatusCallbackArgs arguments)
    {
        CallbackArgs = arguments;
        await OnUserUpdateAsync();
    }
    
    [Parameter] 
    public EventCallback<bool> VisibleChanged { get; set; }
    protected User? User { get; set; }
    protected string? Email { get; set; }
    protected string? Username { get; set; }
    protected bool HasPassword { get; set; }
    protected bool IsEmailConfirmed { get; set; }
    private string? SectionName { get; set; } = "User Profile";

    protected override async Task OnInitializedAsync()
    {
        await OnUserUpdateAsync();
    }
    
    protected async Task OnUserUpdateAsync()
    {
        User = await AuthHelper.GetSelf();
        if (User is null)
        {
            await StatusCallback.InvokeAsync(StatusCallbackArgs.BasicStatusArgs);
            return;
        }
        Email = await UserManager.GetEmailAsync(User);
        HasPassword = await UserManager.HasPasswordAsync(User);
        IsEmailConfirmed = await UserManager.IsEmailConfirmedAsync(User);
        Username = await UserManager.GetUserNameAsync(User);
        StateHasChanged();
    }
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