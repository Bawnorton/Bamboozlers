using System.Text;
using System.Text.Encodings.Web;
using Bamboozlers.Classes;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Services;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

namespace Bamboozlers.Components.Settings;

public partial class CompSettings : SettingsComponentBase
{
    [Parameter] public bool Testing { get; set; }

    [Parameter] public string? SectionName { get; set; }

    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }
    
    private bool _visible;
    [Parameter] public bool Visible { get => _visible; set { if (_visible == value) return; _visible = value; VisibleChanged.InvokeAsync(value); } }
    
    private static StatusArguments Arguments { get; set; }  = new();

    private static async Task OnStatusUpdate(StatusArguments arguments)
    {
        Arguments = arguments;
        await Task.Delay(TimeSpan.FromSeconds(5));
        Arguments = new StatusArguments();
    }

    private async Task<bool> OnDataChange(UserDataRecord userDataRecord)
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
            case UserDataType.Visual: case null: default:
                var user = await GetUser();
                if (user is null) break;
                
                user.DisplayName = userDataRecord.DisplayName != null && userDataRecord.DisplayName != user.DisplayName ? userDataRecord.DisplayName : user.DisplayName;
                user.Avatar = userDataRecord.Avatar != null && userDataRecord.Avatar != user.Avatar ? userDataRecord.Avatar : user.Avatar;
                user.Bio = userDataRecord.Bio != null && userDataRecord.Bio != user.Bio ? userDataRecord.Bio : user.Bio;
                
                await UserManager.UpdateAsync(user);
                result = true;
                break;
        }
        
        if (result)
            await LoadValuesFromStorage();
        return result;
    }

    private static int? UserId { get; set; } = null;
    private async Task<User?> GetUser()
    {
        return await UserManager.FindByIdAsync((await GetUserId()).ToString());
    }
    
    private static async Task<int> GetUserId()
    {
        return UserId ?? (await AuthHelper.GetSelf()).Id;
    }
    protected override async Task OnInitializedAsync()
    {
        SectionName ??= "User Profile";
        await LoadValuesFromStorage();
    }
    
    private async Task<bool> LoadValuesFromStorage()
    {
        var user = await GetUser();
        if (user is null) return false;
        
        UserDisplayRecord.UpdateDisplayRecord(user);
        
        return true;
    }
    
    private async Task<bool> ChangeUsername(string? input, string? pass)
    {
        var user = await GetUser();
        if (user is null)
        {
            await OnStatusUpdate(new StatusArguments(
                Color.Danger,
                true,
                "Could not change your username.",
                "Could not verify your account data."
            ));
            return false;
        }
        
        if (string.IsNullOrEmpty(input))
        {
            await OnStatusUpdate(new StatusArguments(
                Color.Danger,
                true,
                "Could not change your username.",
                "New username must not be an empty field."
            ));
            return false;
        }
        
        if (input == user.UserName)
        {
            await OnStatusUpdate(new StatusArguments(
                statusColor: Color.Danger,
                statusVisible: true,
                statusMessage: "Could not change your username.",
                statusDescription: "New username was the same as current username."
            ));
            return false;  
        }

        var passwordResult = pass is not null && await UserManager.CheckPasswordAsync(user, pass);
        if (passwordResult == false)
        {
            await OnStatusUpdate(new StatusArguments(
                statusColor: Color.Danger,
                statusVisible: true,
                statusMessage: "Error occurred while changing username.",
                statusDescription: "Password entered was incorrect."
            ));
            return false;
        }

        var result = await UserManager.SetUserNameAsync(user, input);
        if (!result.Succeeded)
        {
            await OnStatusUpdate(new StatusArguments(
                statusColor: Color.Danger,
                statusVisible: true,
                statusMessage: "Error occurred while changing username. ",
                statusDescription: $"Error: {string.Join(",", result.Errors.Select(error => error.Description))}"
            ));
            return false;
        }
        
        await UpdateUser(user);
        
        Logger.LogInformation("User changed their username successfully.");
        
        await OnStatusUpdate(new StatusArguments(
            statusColor: Color.Success,
            statusVisible: true,
            statusMessage: "Success! ",
            statusDescription: "Your username has been changed successfully."
        ));
        
        return true;
    }

    private async Task<bool> ChangePassword(string? curp, string? newp)
    {
        var user = await GetUser();
        if (user is null)
        {
            await OnStatusUpdate(new StatusArguments(
                Color.Danger,
                true,
                "Could not change your username.",
                "Could not verify your account data."
            ));
            return false;
        }
        
        if (curp is null || newp is null)
        {
            await OnStatusUpdate(new StatusArguments(
                statusColor: Color.Danger,
                statusVisible: true,
                statusMessage: "Error occurred while changing password.",
                statusDescription: "Current and new password are required."
            ));
            return false;
        }
        
        var result = await UserManager.ChangePasswordAsync(user, curp, newp);
        if (!result.Succeeded)
        {
            await OnStatusUpdate(new StatusArguments(
                statusColor: Color.Danger,
                statusVisible: true,
                statusMessage: "Error occurred while changing password:",
                statusDescription: $"Error: {string.Join(",", result.Errors.Select(error => error.Description))}"
            ));
            return false;
        }
        
        await UpdateUser(user);
        
        Logger.LogInformation("User changed their password successfully.");
        
        await OnStatusUpdate(new StatusArguments(
            statusColor: Color.Success,
            statusVisible: true,
            statusMessage: "Success! ",
            statusDescription: "Your password has been changed successfully."
        ));
        return true;
    }

    private async Task<bool> ChangeEmail(string? newEmail)
    {
        var user = await GetUser();
        if (user is null)
        {
            await OnStatusUpdate(new StatusArguments(
                Color.Danger,
                true,
                "Could not change your username.",
                "Could not verify your account data."
            ));
            return false;
        }
        
        if (newEmail is null || newEmail == user.Email)
        {
            await OnStatusUpdate(new StatusArguments(
                Color.Danger,
                true,
                "Could not change your email.",
                "A different, valid email must be entered to change email."
            ));
            return false;
        }

        await JsRuntime.InvokeVoidAsync("sendNewEmailConfirmation", user.Id, newEmail);

        await OnStatusUpdate(new StatusArguments(
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
            await OnStatusUpdate(new StatusArguments(
                Color.Danger,
                true,
                "Could not change your username.",
                "Could not verify your account data."
            ));
            return false;
        }

        if (!(pass is not null && await UserManager.CheckPasswordAsync(user, pass)))
        {
            await OnStatusUpdate(new StatusArguments(
                Color.Danger,
                true,
                "Error occurred while deleting account.",
                "Incorrect password."
            ));
            return false;
        }
        
        var userId = await UserManager.GetUserIdAsync(user);
        var result = await UserManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            await OnStatusUpdate(new StatusArguments(
                Color.Danger,
                true,
                "Error occurred while deleting account.",
                "Unexpected error occurred while deleting your account."
            ));
            return false;
        }
        
        await JsRuntime.InvokeVoidAsync("forceLogout");
        
        Logger.LogInformation("User with ID '{UserId}' deleted their account.", userId);
        
        return true;
    }

    private async Task UpdateUser(User user)
    {
        await UserManager.UpdateAsync(user);
        await UserManager.UpdateSecurityStampAsync(user);
    }
}