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
using Microsoft.JSInterop;

namespace Bamboozlers.Components.Settings;

public partial class CompSettings : SettingsComponentBase
{
    [Parameter] public string? SectionName { get; set; }

    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }
    
    private bool _visible;
    [Parameter] public bool Visible { get => _visible; set { if (_visible == value) return; _visible = value; VisibleChanged.InvokeAsync(value); } }

    [Parameter] public EventCallback<UserUpdateResult> UserUpdateCallback { get; set; }
    public AlertArguments Arguments { get; set; }  = new();

    public Task OnAlertChange(AlertArguments arguments)
    {
        Arguments = arguments;
        
        StateHasChanged();
        
        return Task.CompletedTask;
    }

    public async Task<bool> OnDataChange(UserDataRecord userDataRecord)
    {
        var result = false;
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
            default:
                var user = await GetUser();
                if (user is null) break;
                
                user.DisplayName = userDataRecord.DisplayName != null && userDataRecord.DisplayName != user.DisplayName ? userDataRecord.DisplayName : user.DisplayName;
                user.Avatar = userDataRecord.Avatar != null && userDataRecord.Avatar != user.Avatar ? userDataRecord.Avatar : user.Avatar;
                user.Bio = userDataRecord.Bio != null && userDataRecord.Bio != user.Bio ? userDataRecord.Bio : user.Bio;
                
                await UserManager.UpdateAsync(user);
                
                await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
                    UserDataType.Visual, 
                    true, 
                    "")
                );
                
                result = true;
                break;
        }
        
        if (result)
            await LoadValuesFromStorage();
        
        StateHasChanged();
        await StateChangedCallback.InvokeAsync();
        
        return result;
    }
    
    private async Task<User?> GetUser()
    {
        return await UserManager.FindByIdAsync((await GetUserId()).ToString());
    }
    
    private static async Task<int> GetUserId()
    {
        var user = await AuthHelper.GetSelf();
        return user?.Id ?? -1;
    }
    protected override async Task OnInitializedAsync()
    {
        SectionName ??= "User Profile";
        await LoadValuesFromStorage();
    }
    
    public async Task LoadValuesFromStorage()
    {
        var user = await GetUser();
        if (user is null) return;
        UserDisplayRecord.UpdateDisplayRecord(user);
    }
    
    private async Task<bool> ChangeUsername(string? input, string? pass)
    {
        var user = await GetUser();
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

        var passwordResult = pass is not null && await UserManager.CheckPasswordAsync(user, pass);
        if (passwordResult == false)
        {
            await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
                UserDataType.Username, 
                false, 
                "Incorrect Password")
            );
            
            await OnAlertChange(new AlertArguments(
                Color.Danger,
                true,
                "Error occurred while changing username.",
                "Password entered was incorrect."
            ));
            return false;
        }

        var result = await UserManager.SetUserNameAsync(user, input);
        if (!result.Succeeded)
        {
            await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
                UserDataType.Username, 
                false, 
                "Error Occurred")
            );
            
            await OnAlertChange(new AlertArguments(
                Color.Danger,
                true,
                "Error occurred while changing username. ",
                $"Error: {string.Join(",", result.Errors.Select(error => error.Description))}"
            ));
            return false;
        }
        
        await UpdateUser(user);
        
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
        
        return true;
    }

    private async Task<bool> ChangePassword(string? curp, string? newp)
    {
        var user = await GetUser();
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
        
        if (curp is null || newp is null)
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
        
        var result = await UserManager.ChangePasswordAsync(user, curp, newp);
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
                $"Error: {string.Join(",", result.Errors.Select(error => error.Description))}"
            ));
            return false;
        }
        
        await UpdateUser(user);
        
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
        return true;
    }

    private async Task<bool> ChangeEmail(string? newEmail)
    {
        var user = await GetUser();
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
        
        var userId = await GetUserId();
        await JsRuntime.InvokeVoidAsync("sendNewEmailConfirmation", userId, newEmail);

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
        var user = await GetUser();
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

        if (!(pass is not null && await UserManager.CheckPasswordAsync(user, pass)))
        {
            await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
                UserDataType.Deletion, 
                false, 
                "Incorrect Password")
            );
            
            await OnAlertChange(new AlertArguments(
                Color.Danger,
                true,
                "Error occurred while deleting account.",
                "Incorrect password."
            ));
            return false;
        }

        AuthHelper.InvalidateAuthState();
        var result = await UserManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
                UserDataType.Deletion, 
                false, 
                "Unknown")
            );
            await OnAlertChange(new AlertArguments(
                Color.Danger,
                true,
                "Error occurred while deleting account.",
                "Unexpected error occurred while deleting your account."
            ));
            return false;
        }
        
        await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
            UserDataType.Deletion, 
            true, 
            "")
        );
        
        Logger.LogInformation("User with ID '{GetUserId()}' deleted their account.", GetUserId());
        
        await JsRuntime.InvokeVoidAsync("forceLogout");
        
        return true;
    }

    private async Task UpdateUser(User user)
    {
        AuthHelper.InvalidateAuthState();
        await UserManager.UpdateAsync(user);
        await UserManager.UpdateSecurityStampAsync(user);
    }
}