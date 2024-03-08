using System.Text;
using System.Text.Encodings.Web;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Services;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace Bamboozlers.Components.Settings;

public partial class CompSettings : SettingsComponentBase
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
            default:
                var user = await GetUser();
                if (user is null) 
                    break;
                user.DisplayName = userModel.DisplayName != null && userModel.DisplayName != user.DisplayName ? userModel.DisplayName : user.DisplayName;
                user.Avatar = userModel.Avatar != null && userModel.Avatar != user.Avatar ? userModel.Avatar : user.Avatar;
                user.Bio = userModel.Bio != null && userModel.Bio != user.Bio ? userModel.Bio : user.Bio;
                await UserManager.UpdateAsync(user);
                break;
        }
        await LoadValuesFromStorage();
    }

    private static int UserId { get; set; } = -1;
    
    protected override async Task OnInitializedAsync()
    {
        SectionName ??= "User Profile";
        UserId = await GetUserId();
        await LoadValuesFromStorage();
    }

    private async Task LoadValuesFromStorage()
    {
        var user = await GetUser();
        await UserModel.UpdateUserModel(user);
    }

    private async Task<User?> GetUser()
    {
        return await UserManager.FindByIdAsync(UserId.ToString());
    }
    
    private static async Task<int> GetUserId()
    {
        if (UserId != -1) return UserId;
        var temp = await AuthHelper.GetSelf();
        return temp.Id;
    }

    private async Task ChangeUsername(string? input, string? pass)
    {
        var user = await GetUser();
        if (user is null)
        {
            UpdateStatusArgs(new StatusArguments(
                Color.Danger,
                true,
                "Could not change your username.",
                "Could not verify your account data."
            ));
            return;
        }
        
        if (string.IsNullOrEmpty(input))
        {
            UpdateStatusArgs(new StatusArguments(
                Color.Danger,
                true,
                "Could not change your username.",
                "New username must not be an empty field."
            ));
            return;
        }
        
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

        var passwordResult = pass is not null && await UserManager.CheckPasswordAsync(user, pass);
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
        
        await UpdateUser(user);
        
        Logger.LogInformation("User changed their username successfully.");
        
        UpdateStatusArgs(new StatusArguments(
            statusColor: Color.Success,
            statusVisible: true,
            statusMessage: "Success! ",
            statusDescription: "Your username has been changed successfully."
        ));
    }

    private async Task ChangePassword(string? curp, string? newp)
    {
        var user = await GetUser();
        if (user is null)
        {
            UpdateStatusArgs(new StatusArguments(
                Color.Danger,
                true,
                "Could not change your username.",
                "Could not verify your account data."
            ));
            return;
        }
        
        if (curp is null || newp is null)
        {
            UpdateStatusArgs(new StatusArguments(
                statusColor: Color.Danger,
                statusVisible: true,
                statusMessage: "Error occurred while changing password.",
                statusDescription: "Current and new password are required."
            ));
            return;
        }
        
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
        
        await UpdateUser(user);
        
        Logger.LogInformation("User changed their password successfully.");
        
        UpdateStatusArgs(new StatusArguments(
            statusColor: Color.Success,
            statusVisible: true,
            statusMessage: "Success! ",
            statusDescription: "Your password has been changed successfully."
        ));
    }

    private async Task ChangeEmail(string? newEmail)
    {
        var user = await GetUser();
        if (user is null)
        {
            UpdateStatusArgs(new StatusArguments(
                Color.Danger,
                true,
                "Could not change your username.",
                "Could not verify your account data."
            ));
            return;
        }
        
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
    }

    private async Task DeleteAccount(string? pass)
    {
        var user = await GetUser();
        if (user is null)
        {
            UpdateStatusArgs(new StatusArguments(
                Color.Danger,
                true,
                "Could not change your username.",
                "Could not verify your account data."
            ));
            return;
        }

        if (!(pass is not null && await UserManager.CheckPasswordAsync(user, pass)))
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
        await UpdateUser(user);
    }

    private async Task UpdateUser(User user)
    {
        await UserManager.UpdateAsync(user);
        await UserManager.UpdateSecurityStampAsync(user);
    }
}