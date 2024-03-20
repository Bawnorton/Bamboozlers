using System.Text;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Text.Json;
using Bamboozlers.Classes;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Data.ViewModel;
using Bamboozlers.Classes.Services;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;

namespace Bamboozlers.Components.Settings;

public partial class CompSettings : SettingsComponentBase
{
    [Parameter] public bool Visible { get; set; }
    [Parameter] public string? SectionName { get; set; }
    [Parameter] public string? SentStatusMessage { get; set; }
    [Parameter] public string? SentStatusDescription { get; set; }
    
    protected override void OnInitialized()
    {
        SectionName ??= "User Profile";
    }

    // TODO: Impl with events?
    protected override async Task OnParametersSetAsync()
    {
        if (!(SentStatusMessage is null || SentStatusDescription is null))
        {
            await OnAlertChange(new AlertArguments(
                Color.Default,
                true,
                SentStatusMessage,
                SentStatusDescription
            ));
        }
    }

    [Parameter] public EventCallback<UserUpdateResult> UserUpdateCallback { get; set; }
    public AlertArguments Arguments { get; private set; }  = new();

    public Task OnAlertChange(AlertArguments arguments)
    {
        Arguments = arguments;
        
        StateHasChanged();
        
        return Task.CompletedTask;
    }

    public async Task<bool> OnDataChange(UserDataRecord userDataRecord)
    {
        bool result;
        switch (userDataRecord.DataType)
        {
            case UserDataType.Username: 
                result = await ChangeUsername(userDataRecord.UserName, userDataRecord.CurrentPassword);
                break;
            case UserDataType.Password:
                result = await ChangePassword(userDataRecord.CurrentPassword, userDataRecord.NewPassword);
                break;
            case UserDataType.Deletion:
                result = await DeleteAccount(userDataRecord.CurrentPassword);
                if (result) return result;
                break;
            case UserDataType.Email:
                result = await ChangeEmail(userDataRecord.Email);
                break;
            case UserDataType.Visual:
            case null:
            default:
                var iResult = await UserService.UpdateUserAsync(userDataRecord);
                
                await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
                    UserDataType.Visual, 
                    iResult.Succeeded, 
                    iResult.Succeeded ? "" : $"Error: {string.Join(",", iResult.Errors.Select(error => error.Description))}")
                );
                
                result = iResult.Succeeded;
                break;
        }
        
        StateHasChanged();
        await StateChangedCallback.InvokeAsync();
        
        return result;
    }
    
    private async Task<bool> ChangeUsername(string? input, string? pass)
    {
        var user = await UserService.GetUserDataAsync();
        if (user is null)
        {
            await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
                UserDataType.Username, 
                false, 
                "User not found")
            );
            
            await OnAlertChange(new AlertArguments(
                Color.Danger,
                true,
                "Could not change your username.",
                "Could not verify your account data."
            ));
            return false;
        }
        
        if (string.IsNullOrEmpty(input))
        {
            await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
                UserDataType.Username, 
                false, 
                "Invalid Or Empty")
            );
            
            await OnAlertChange(new AlertArguments(
                Color.Danger,
                true,
                "Could not change your username.",
                "New username must not be an empty field."
            ));
            return false;
        }
        
        if (input == user.UserName)
        {
            await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
                UserDataType.Username, 
                false, 
                "Same Username")
            );
            
            await OnAlertChange(new AlertArguments(
                Color.Danger,
                true,
                "Could not change your username.",
                "New username was the same as current username."
            ));
            return false;  
        }
        
        if (pass is null)
        {
            await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
                UserDataType.Username, 
                false, 
                "Empty Password")
            );
            
            await OnAlertChange(new AlertArguments(
                Color.Danger,
                true,
                "Error occurred while changing username.",
                "Password is required."
            ));
            return false;
        }

        var result = await UserService.ChangeUsernameAsync(input, pass);
        if (!result.Succeeded)
        {
            var errorString = $"Error: {string.Join(",", result.Errors.Select(error => error.Description))}";
            await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
                UserDataType.Username, 
                false, 
                errorString)
            );
            
            await OnAlertChange(new AlertArguments(
                Color.Danger,
                true,
                "Error occurred while changing username. ",
                errorString
            ));
            return false;
        }
        
        await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
            UserDataType.Username, 
            true, 
            "")
        );
        
        Logger.LogInformation("User changed their username successfully.");
        
        await OnAlertChange(new AlertArguments(
            Color.Success,
            true,
            "Success! ",
            "Your username has been changed successfully."
        ));
        
        await JsRuntime.InvokeVoidAsync("Reauthenticate");
        
        return true;
    }

    private async Task<bool> ChangePassword(string? currentPassword, string? newPassword)
    {
        var user = await UserService.GetUserDataAsync();
        if (user is null)
        {
            await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
                UserDataType.Password, 
                false, 
                "User not found")
            );
            
            await OnAlertChange(new AlertArguments(
                Color.Danger,
                true,
                "Could not change your username.",
                "Could not verify your account data."
            ));
            return false;
        }
        
        if (currentPassword is null || newPassword is null)
        {
            await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
                UserDataType.Password, 
                false, 
                "Missing Input")
            );
            
            await OnAlertChange(new AlertArguments(
                Color.Danger,
                true,
                "Error occurred while changing password.",
                "Current and new password are required."
            ));
            return false;
        }
        
        var result = await UserService.ChangePasswordAsync( currentPassword, newPassword);
        if (!result.Succeeded)
        {
            var errorString = $"Error: {string.Join(",", result.Errors.Select(error => error.Description))}";
            await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
                UserDataType.Password, 
                false, 
                errorString)
            );
            
            await OnAlertChange(new AlertArguments(
                Color.Danger,
                true,
                "Error occurred while changing password:",
                errorString
            ));
            return false;
        }
        
        await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
            UserDataType.Password, 
            true, 
            "")
        );
        
        Logger.LogInformation("User changed their password successfully.");
        
        await OnAlertChange(new AlertArguments(
            Color.Success,
            true,
            "Success! ",
            "Your password has been changed successfully."
        ));
        
        await JsRuntime.InvokeVoidAsync("settingsInterop.Reauthenticate");
        
        return true;
    }

    private async Task<bool> ChangeEmail(string? newEmail)
    {
        var user = await UserService.GetUserDataAsync();
        if (user is null)
        {
            await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
                UserDataType.Email, 
                false, 
                "User not found")
            );
            
            await OnAlertChange(new AlertArguments(
                Color.Danger,
                true,
                "Could not change your username.",
                "Could not verify your account data."
            ));
            return false;
        }
        
        if (newEmail is null || newEmail == user.Email)
        {
            await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
                UserDataType.Email, 
                false, 
                "Email Invalid Or Same")
            );
            
            await OnAlertChange(new AlertArguments(
                Color.Danger,
                true,
                "Could not change your email.",
                "A different, valid email must be entered to change email."
            ));
            return false;
        }

        await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
            UserDataType.Email, 
            true, 
            "")
        );
        
        await JsRuntime.InvokeVoidAsync("settingsInterop.SendNewEmailConfirmation", user.Id, newEmail);

        await OnAlertChange(new AlertArguments(
            Color.Secondary,
            true,
            "Confirmation link was sent to new email.",
            "Please check your inbox to confirm changes."
        ));
        
        return true;
    }

    private async Task<bool> DeleteAccount(string? pass)
    {
        var user = await UserService.GetUserDataAsync();
        
        if (user is null)
        {
            await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
                UserDataType.Deletion, 
                false, 
                "User not found")
            );
            
            await OnAlertChange(new AlertArguments(
                Color.Danger,
                true,
                "Could not change your username.",
                "Could not verify your account data."
            ));
            return false;
        }
        
        if (pass is null)
        {
            await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
                UserDataType.Deletion, 
                false, 
                "Empty Password")
            );
            
            await OnAlertChange(new AlertArguments(
                Color.Danger,
                true,
                "Error occurred while changing username.",
                "Password is required."
            ));
            return false;
        }
        
        var result = await UserService.DeleteAccountAsync(pass);
        if (!result.Succeeded)
        {
            var errorString = $"Error: {string.Join(",", result.Errors.Select(error => error.Description))}";
            await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
                UserDataType.Deletion, 
                false, 
                errorString)
            );
            
            await OnAlertChange(new AlertArguments(
                Color.Danger,
                true,
                "Error occurred while changing password:",
                errorString
            ));
            return false;
        }
        
        await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
            UserDataType.Deletion, 
            true, 
            "")
        );
        
        Logger.LogInformation("User with name '{user.UserName}' deleted their account.",user.UserName);
        
        await JsRuntime.InvokeVoidAsync("settingsInterop.ForceLogout");
        
        return true;
    }
}