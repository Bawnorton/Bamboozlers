using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Data.ViewModel;
using Microsoft.AspNetCore.Identity;

namespace Bamboozlers.Classes.Services;

public static class UserService
{
    private static ServiceProviderWrapper _services;
    
    public static async Task Init(ServiceProviderWrapper serviceProvider)
    {
        _services = serviceProvider;
        await UpdateDisplayRecordAsync();
    }
    
    /// <summary>
    /// Retrieve User data in UserDataRecord form to prevent query-tracking related errors.
    /// </summary>
    public static async Task<UserDataRecord?> GetUserDataAsync()
    {
        var user = await AuthHelper.GetSelf();
        if (user is null)
            return null;
        
        return new UserDataRecord(
            user.Id, 
            user.UserName, 
            user.Email, 
            user.DisplayName, 
            user.Bio, 
            user.Avatar
        );
    }

    /// <summary>
    /// Get the user ID, used when querying the User from UserManager to prevent query-tracking related errors.
    /// </summary>
    private static async Task<string?> GetUserIdAsync()
    {
        /* It's insane that the workaround to the tracking error is to get the user Id and use that to query from the
           UserManager
        */
        return (await AuthHelper.GetSelf())?.Id.ToString();
    }
    
    /// <summary>
    /// Update the user's representation in the database.
    /// </summary>
    /// <param name="newValues">
    /// The values, if any, of the user to be updated.
    /// </param>
    public static async Task<IdentityResult> UpdateUserAsync(UserDataRecord? newValues = null)
    {
        using var scope = _services.CreateScope();
        
        var userId = await GetUserIdAsync();
        if (userId is null) 
            return IdentityResult.Failed([new IdentityError { Description = "User not found." }]);
        
        var userManager = _services.GetService<UserManager<User>>();
        if (userManager is null)
            return IdentityResult.Failed([new IdentityError { Description = "Could not obtain User Manager service." }]);

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) 
            return IdentityResult.Failed([new IdentityError { Description = "User not found." }]);
        
        if (newValues is not null)
        {
            user.DisplayName = newValues.DisplayName ?? user.DisplayName;
            user.Bio = newValues.Bio ?? user.Bio;
            user.Avatar = newValues.Avatar ?? user.Avatar;
        }
        
        var iResult = await userManager.UpdateAsync(user);
        
        AuthHelper.Invalidate();
        await UpdateDisplayRecordAsync();
        
        return iResult;
    }

    /// <summary>
    /// Update the user's username in the database, and updates (invalidate) the security stamp for the user.
    /// </summary>
    /// <params>
    /// <param name="username">
    /// The inputted new username.
    /// </param>
    /// <param name="password">
    /// The inputted password that should match the user's current password.
    /// </param>
    /// </params>
    /// <returns>
    /// An IdentityResult, indicating either success, or an error with a description of the issue.
    /// </returns>
    public static async Task<IdentityResult> ChangeUsernameAsync(string username, string password)
    {
        using var scope = _services.CreateScope();
        
        var userId = await GetUserIdAsync();
        if (userId is null) 
            return IdentityResult.Failed([new IdentityError { Description = "User not found." }]);
        
        var userManager = _services.GetService<UserManager<User>>();
        if (userManager is null)
            return IdentityResult.Failed([new IdentityError { Description = "Could not obtain User Manager service." }]);

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) 
            return IdentityResult.Failed([new IdentityError { Description = "User not found." }]);

        var res = await userManager.CheckPasswordAsync(user, password);
        if (!res) return IdentityResult.Failed([new IdentityError { Description = "Password was incorrect." }]);
        
        var iResult = await userManager.SetUserNameAsync(user, username);
        if (!iResult.Succeeded) return iResult;
        
        iResult = await userManager.UpdateAsync(user);
        if (!iResult.Succeeded) return iResult;

        iResult = await userManager.UpdateSecurityStampAsync(user);
        AuthHelper.Invalidate();
        return iResult;
    }
    
    /// <summary>
    /// Update the user's password in the database, and updates (invalidate) the security stamp for the user.
    /// </summary>
    /// <params>
    /// <param name="currentPassword">
    /// The inputted password that should match the user's current password.
    /// </param>
    /// <param name="newPassword">
    /// The inputted new password.
    /// </param>
    /// </params>
    /// <returns>
    /// An IdentityResult, indicating either success, or an error with a description of the issue.
    /// </returns>
    public static async Task<IdentityResult> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        using var scope = _services.CreateScope();
        
        var userId = await GetUserIdAsync();
        if (userId is null) 
            return IdentityResult.Failed([new IdentityError { Description = "User not found." }]);
        
        var userManager = _services.GetService<UserManager<User>>();
        if (userManager is null)
            return IdentityResult.Failed([new IdentityError { Description = "Could not obtain User Manager service." }]);

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) 
            return IdentityResult.Failed([new IdentityError { Description = "User not found." }]);
        
        var iResult = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!iResult.Succeeded) return iResult;
        
        iResult = await userManager.UpdateAsync(user);
        if (!iResult.Succeeded) return iResult;

        iResult = await userManager.UpdateSecurityStampAsync(user);
        AuthHelper.Invalidate();
        return iResult;
    }
    
    /// <summary>
    /// Deletes the user's account.
    /// </summary>
    /// <param name="password">
    /// The inputted password that should match the user's current password.
    /// </param>
    /// <returns>
    /// An IdentityResult, indicating either success, or an error with a description of the issue.
    /// </returns>
    public static async Task<IdentityResult> DeleteAccountAsync(string password)
    {
        using var scope = _services.CreateScope();
        
        var userId = await GetUserIdAsync();
        if (userId is null) 
            return IdentityResult.Failed([new IdentityError { Description = "User not found." }]);
        
        var userManager = _services.GetService<UserManager<User>>();
        if (userManager is null)
            return IdentityResult.Failed([new IdentityError { Description = "Could not obtain User Manager service." }]);

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) 
            return IdentityResult.Failed([new IdentityError { Description = "User not found." }]);

        var res = await userManager.CheckPasswordAsync(user, password);
        if (!res) return IdentityResult.Failed([new IdentityError { Description = "Password was incorrect." }]);

        var iResult = await userManager.DeleteAsync(user);
        
        return iResult;
    }

    /// <summary>
    /// Updates the display record to align with the user's current database representation.
    /// </summary>
    public static async Task UpdateDisplayRecordAsync()
    {
        var user = await AuthHelper.GetSelf();
        UserDisplayRecord.Update(user);
    }
}