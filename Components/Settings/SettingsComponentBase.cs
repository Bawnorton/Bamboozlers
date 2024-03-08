/* BASE CLASS FOR SETTINGS COMPONENTS */

using Bamboozlers.Classes.AppDbContext;
using Blazorise;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Components.Settings;

public class SettingsComponentBase : ComponentBase
{
    protected static StatusArguments Arguments { get; private set; }  = new(Color.Default,false,"","");
    
    protected static void UpdateStatusArgs(StatusArguments arguments)
    {
        Arguments = arguments;
    }

    [Parameter] 
    public EventCallback<UserModel> DataChangeCallback { get; set; }
}

public sealed class StatusArguments(Color statusColor, bool statusVisible, string statusMessage, string statusDescription)
{
    public static readonly StatusArguments BasicStatusArgs = new (
        Color.Danger,
        true,
        "Could not find your account details.",
        "Your user data could not be verified at this time."
    );
    
    public readonly Color StatusColor = statusColor;
    public readonly bool StatusVisible = statusVisible;
    public readonly string StatusMessage = statusMessage;
    public readonly string StatusDescription = statusDescription;
}

public enum DataChangeType
{
    Password,
    Username,
    Deletion,
    Email,
    Visual
}

public sealed class UserModel
{
    public static UserModel U { get; private set; } = new();
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public /*string?*/ byte[]? Avatar { get; set; }
    
    /* Alarming, I know, but these fields are for the purposes of transferring via a callback from earlier components */

    public DataChangeType? Type { get; set; }
    public string? Password { get; set; }
    public string? NewPassword { get; set; }

    public static async Task UpdateUserModel(User? user)
    {
        if (user is null) return;
        U.UserName = user.UserName;
        U.Email = user.Email;
        U.DisplayName = user.DisplayName;
        U.Bio = user.Bio;
        U.Avatar = user.Avatar;
    }
}