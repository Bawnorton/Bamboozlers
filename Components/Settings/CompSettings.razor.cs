using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Reflection;
using System.Runtime.CompilerServices;
using Azure.Identity;
using Bamboozlers.Account;
using Bamboozlers.Classes;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Services;
using Bamboozlers.Components.Settings;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

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

    private async Task OnStatusUpdate(StatusCallbackArgs arguments)
    {
        CallbackArgs = arguments;
    }
    
    [Parameter] 
    public EventCallback UserUpdateCallback { get; set; }

    private async Task OnUserUpdate()
    {
        await OnUserUpdateAsync();
    }

    [Parameter] 
    public EventCallback<bool> VisibleChanged { get; set; }
    protected User? User { get; set; }
    protected string? Email { get; set; }
    protected string? Username { get; set; }
    protected string? DisplayName { get; set; }
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
        DisplayName = User.DisplayName;
        await SaveDatabaseChanges();
        StateHasChanged();
    }

    private async Task SaveDatabaseChanges()
    {
        if (User is null)
        {
            await StatusCallback.InvokeAsync(StatusCallbackArgs.BasicStatusArgs);
            return;
        }
        var userId = await UserManager.GetUserIdAsync(User);
        await using var db = await Db.CreateDbContextAsync();
        
        if (User is not null)
        {
            var match = db.Users.Single(u => u.Id == int.Parse(userId));
            foreach (var property in User.GetType().GetProperties())
            {
                var dbInfo = match.GetType().GetProperty(property.Name);
                if (dbInfo is null) continue;
                
                var oldProperty = property.GetValue(match);
                if (oldProperty is null) continue;
                
                var newProperty = property.GetValue(User);

                if (!oldProperty.Equals(newProperty))
                {
                    property.SetValue(match, property.GetValue(User));
                }
            }
            await db.SaveChangesAsync();
        }
        else
        {
            await StatusCallback.InvokeAsync(new StatusCallbackArgs(
                Color.Danger,
                true,
                "Could not save changes.",
                "Your user data could not be saved at this time."
            ));
        }
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