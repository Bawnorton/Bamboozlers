using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Networking.Packets;
using Bamboozlers.Classes.Networking.Packets.Serverbound.User;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.JSInterop;

namespace Bamboozlers.Components.Settings;

public partial class CompSettings : SettingsComponentBase
{
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private IDbContextFactory<AppDbContext> Db { get; set; } = default!;
    [Parameter] public bool Visible { get; set; }
    [Parameter] public string? SectionName { get; set; }
    
    [CascadingParameter] public EventCallback<IPacket> SendToServer { get; set; }
    
    protected override void OnInitialized()
    {
        SectionName ??= "Account";
    }

    [Parameter] public EventCallback<UserUpdateResult> UserUpdateCallback { get; set; }
    public AlertArguments Arguments { get; private set; }  = new();

    public async Task OnAlertChange(AlertArguments arguments)
    {
        Arguments = arguments;
        await InvokeAsync(StateHasChanged);
    }

    public async Task<bool> OnDataChange(UserDataRecord userDataRecord)
    {
        var userData = await UserService.GetUserDataAsync();
        var username = userData.UserName!;
        var sync = await DataChangeInternal(userDataRecord);
        if (sync)
        {
            var syncUserData = new UserDataSyncC2SPacket
            {
                UserId = userData.Id!.Value,
                Username = username
            };
            await SendToServer.InvokeAsync(syncUserData);
        }
        await InvokeAsync(StateHasChanged);
        return sync;
    }
    
    private async Task<bool> DataChangeInternal(UserDataRecord userDataRecord)
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
            {
                await using var db = await Db.CreateDbContextAsync();
                var userId = (await UserService.GetUserDataAsync()).Id!.Value;
                var friends = db.FriendShips
                    .AsQueryable()
                    .ToList()
                    .Where(f => f.User1ID == userId || f.User2ID == userId)
                    .Select(f => f.User1ID == userId ? f.User2ID : f.User1ID);
                var potentialFriends = db.FriendRequests
                    .AsQueryable()
                    .ToList()
                    .Where(f => f.ReceiverID == userId)
                    .Select(f => f.SenderID);
                potentialFriends = potentialFriends.Concat(db.FriendRequests
                    .AsQueryable()
                    .ToList()
                    .Where(f => f.SenderID == userId)
                    .Select(f => f.ReceiverID));
                var chatUsers = db.Chats
                    .Include(c => c.Users)
                    .AsQueryable()
                    .ToList()
                    .Where(c => c.Users.Any(u => u.Id == userId))
                    .SelectMany(c => c.Users)
                    .Select(u => u.Id);
                    
                var toNotify = new List<int>();
                toNotify.AddRange(friends);
                toNotify.AddRange(potentialFriends);
                toNotify.AddRange(chatUsers);
                toNotify = toNotify.Distinct().ToList();
                
                result = await DeleteAccount(userDataRecord.CurrentPassword);
                if (!result) return false;
                
                var userDeleted = new UserDeletedC2SPacket
                {
                    UserId = userId,
                    ToNotify = toNotify
                };
                await SendToServer.InvokeAsync(userDeleted);

                return false;  // dont sync twice
            }
            case UserDataType.Email:
                result = await ChangeEmail(userDataRecord.Email);
                break;
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
        
        return result;
    }

    private async Task<bool> ChangeUsername(string? input, string? pass)
    {
        UserData = await UserService.GetUserDataAsync();
        if (UserData is null || UserData == UserRecord.Default)
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

        if (input == UserData.UserName)
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

        await OnAlertChange(new AlertArguments(
            Color.Success,
            true,
            "Success! ",
            "Your username has been changed successfully."
        ));

        await JsRuntime.InvokeVoidAsync("settingsInterop.Reauthenticate");

        return true;
    }

    private async Task<bool> ChangePassword(string? currentPassword, string? newPassword)
    {
        UserData = await UserService.GetUserDataAsync();
        if (UserData is null || UserData == UserRecord.Default)
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

        var result = await UserService.ChangePasswordAsync(currentPassword, newPassword);
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
        UserData = await UserService.GetUserDataAsync();
        if (UserData is null || UserData == UserRecord.Default)
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

        if (newEmail is null || newEmail == UserData.Email)
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

        await JsRuntime.InvokeVoidAsync("settingsInterop.SendNewEmailConfirmation", UserData.Id, newEmail);

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
        UserData = await UserService.GetUserDataAsync();
        if (UserData is null || UserData == UserRecord.Default)
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
                "Error occurred while deleting account. ",
                errorString
            ));
            return false;
        }

        await UserUpdateCallback.InvokeAsync(new UserUpdateResult(
            UserDataType.Deletion,
            true,
            "")
        );

        await JsRuntime.InvokeVoidAsync("settingsInterop.ForceLogout");

        return true;
    }
}