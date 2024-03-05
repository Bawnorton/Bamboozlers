using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using Azure.Identity;
using Bamboozlers.Account;
using Bamboozlers.Classes;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Services;
using Bamboozlers.Components.Settings;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Components.Settings;

public partial class CompSettings : ComponentBase
{
    [Parameter]
    public string? SectionName { get; set; }
    
    [Parameter] 
    public EventCallback<bool> VisibleChanged { get; set; }
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
    
    private static StatusArguments Arguments { get; set; }  = new();
    
    protected static void UpdateStatusArgs(StatusArguments arguments)
    {
        Arguments = arguments;
    }
    
    [Parameter] 
    public EventCallback<UserModel> DataChangeCallback { get; set; }

    private async Task OnDataChange(UserModel userModel)
    {
        switch (userModel.Type)
        {
            case DataChangeType.Username: 
                await ChangeUsername(userModel.UserName, userModel.Password);
                break;
            case DataChangeType.Password:
                await ChangePassword(userModel.Password, userModel.NewPassword);
                break;
            case DataChangeType.Deletion:
                await DeleteAccount(userModel.Password);
                break;
            case DataChangeType.Email:
                await ChangeEmail(userModel.Email);
                break;
            case DataChangeType.Visual:
                var user = await GetUser();
                user.DisplayName = userModel.DisplayName != null && userModel.DisplayName != user.DisplayName ? userModel.DisplayName : user.DisplayName;
                user.Avatar = userModel.Avatar != null && userModel.Avatar != user.Avatar ? userModel.Avatar : user.Avatar;
                user.Bio = userModel.Bio != null && userModel.Bio != user.Bio ? userModel.Bio : user.Bio;
                await UserManager.UpdateAsync(user);
                UserManager.Dispose();
                break;
        }
        await LoadValuesFromStorage();
    }

    protected override async Task OnInitializedAsync()
    {
        SectionName ??= "User Profile";
        await LoadValuesFromStorage();
    }

    private async Task LoadValuesFromStorage()
    {
        var user = await GetUser();
        await UserModel.UpdateUserModel(user);
    }

    protected async Task<User> GetUser()
    {
        return await AuthHelper.GetSelf();
    }

    private async Task ChangeUsername(string input, string pass)
    {
        var user = await GetUser();
        if (input == user.UserName)
        {
            UpdateStatusArgs(new StatusArguments(
                statusColor: Color.Danger,
                statusVisible: true,
                statusMessage: "Could not change your username.",
                statusDescription: "New username was the same as current username."
            ));
            return;  
        }
        
        var passwordResult = await UserManager.CheckPasswordAsync(user, pass);
        if (passwordResult == false)
        {
            UpdateStatusArgs(new StatusArguments(
                statusColor: Color.Danger,
                statusVisible: true,
                statusMessage: "Error occurred while changing username.",
                statusDescription: "Password entered was incorrect."
            ));
            return;
        }

        var result = await UserManager.SetUserNameAsync(user, input);
        if (!result.Succeeded)
        {
            UpdateStatusArgs(new StatusArguments(
                statusColor: Color.Danger,
                statusVisible: true,
                statusMessage: "Error occurred while changing username. ",
                statusDescription: $"Error: {string.Join(",", result.Errors.Select(error => error.Description))}"
            ));
        }
        await UserManager.UpdateAsync(user);
        await UserManager.UpdateSecurityStampAsync(user);
        Logger.LogInformation("User changed their username successfully.");
        
        UpdateStatusArgs(new StatusArguments(
            statusColor: Color.Success,
            statusVisible: true,
            statusMessage: "Success! ",
            statusDescription: $"Your username has been changed successfully."
        ));
        UserManager.Dispose();
    }

    private async Task ChangePassword(string curp, string newp)
    {
        var user = await GetUser();
        var result = await UserManager.ChangePasswordAsync(user, curp, newp);
        if (!result.Succeeded)
        {
            UpdateStatusArgs(new StatusArguments(
                statusColor: Color.Danger,
                statusVisible: true,
                statusMessage: "Error occurred while changing password:",
                statusDescription: $"Error: {string.Join(",", result.Errors.Select(error => error.Description))}"
            ));
        }
        await UserManager.UpdateAsync(user);
        await UserManager.UpdateSecurityStampAsync(user);
        Logger.LogInformation("User changed their password successfully.");
        
        UpdateStatusArgs(new StatusArguments(
            statusColor: Color.Success,
            statusVisible: true,
            statusMessage: "Success! ",
            statusDescription: "Your password has been changed successfully."
        ));
        UserManager.Dispose();
    }

    private async Task ChangeEmail(string? newEmail)
    {
        var user = await GetUser();
        
        if (newEmail is null || newEmail == user.Email)
        {
            UpdateStatusArgs(new StatusArguments(
                Color.Danger,
                true,
                "Could not change your email.",
                "A different, valid email must be entered to change email."
            ));
            return;
        }

        var userId = await UserManager.GetUserIdAsync(user);
        var code = await UserManager.GenerateChangeEmailTokenAsync(user, newEmail);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = NavigationManager.GetUriWithQueryParameters(
            NavigationManager.ToAbsoluteUri("Account/ConfirmEmailChange").AbsoluteUri,
            new Dictionary<string, object?> { ["userId"] = userId, ["email"] = newEmail, ["code"] = code });

        await EmailSender.SendConfirmationLinkAsync(user, newEmail, HtmlEncoder.Default.Encode(callbackUrl));

        UpdateStatusArgs(new StatusArguments(
            Color.Secondary,
            true,
            "Confirmation link was sent to new email.",
            "Please check your inbox to confirm changes."
        ));
        UserManager.Dispose();
    }

    private async Task DeleteAccount(string pass)
    {
        var user = await GetUser();

        if (!await UserManager.CheckPasswordAsync(user, pass))
        {
            UpdateStatusArgs(new StatusArguments(
                Color.Danger,
                true,
                "Error occurred while deleting account.",
                "Incorrect password."
            ));
            return;
        }
        
        var userId = await UserManager.GetUserIdAsync(user);
        var result = await UserManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            UpdateStatusArgs(new StatusArguments(
                Color.Danger,
                true,
                "Error occurred while deleting account.",
                "Unexpected error occurred while deleting your account."
            ));
            throw new InvalidOperationException("Unexpected error occurred deleting user.");
        }
        
        Logger.LogInformation("User with ID '{UserId}' deleted their account.", userId);
        await UserManager.UpdateAsync(user);
        await UserManager.UpdateSecurityStampAsync(user);
        UserManager.Dispose();
    }
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
    public UserModel() { }

    public static async Task UpdateUserModel(User user)
    {
        U.UserName = user.UserName;
        U.Email = user.Email;
        U.DisplayName = user.DisplayName;
        U.Bio = user.Bio;
        U.Avatar = user.Avatar;
    }
}

public sealed class StatusArguments
{
    public static readonly StatusArguments BasicStatusArgs = new (
        Color.Danger,
        true,
        "Could not find your account details.",
        "Your user data could not be verified at this time."
    );
    
    public readonly Color StatusColor;
    public readonly string StatusDescription;
    public readonly string StatusMessage;
    public readonly bool StatusVisible;

    public StatusArguments()
    {
        StatusColor = Color.Default;
        StatusVisible = false;
        StatusMessage = "";
        StatusDescription = "";
    }

    public StatusArguments(Color statusColor, bool statusVisible, string statusMessage, string statusDescription)
    {
        StatusColor = statusColor;
        StatusVisible = statusVisible;
        StatusMessage = statusMessage;
        StatusDescription = statusDescription;
    }
}