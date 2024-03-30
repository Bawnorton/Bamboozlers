using System.Security.Claims;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Utility.Observer;
using Microsoft.AspNetCore.Identity;

namespace Bamboozlers.Classes.Services.Authentication;

public class UserService(IAuthService authService, UserManager<User> userManager) : IUserService
{
    private IAuthService AuthService { get; } = authService;
    private UserManager<User> UserManager { get; } = userManager;

    /* User (Data) Retrieval */
    private UserRecord? UserRecord { get; set; }
    
    /// <summary>
    /// Gets an awaitable task to retrieve the User from the UserManager service.
    /// </summary>
    /// <param name="claimsPrincipal">
    /// The claims principal corresponding to the desired user.
    /// </param>
    /// <returns>
    /// An awaitable task that returns the desired user, or null if the user is not found based on the claims principal.
    /// </returns>
    public virtual Task<User?> GetUserAsync(ClaimsPrincipal claimsPrincipal)
    {
        return UserManager.GetUserAsync(claimsPrincipal);
    }

    /// <summary>
    /// Builds a display record corresponding to the current user.
    /// </summary>
    private async Task<UserRecord> BuildUserDataAsync()
    {
        var claims = await AuthService.GetClaims();
        var user = await UserManager.GetUserAsync(claims);
        
        var record = user is not null
        ? new UserRecord(
            user.Id,
            user.UserName,
            user.Email,
            user.DisplayName,
            user.Bio,
            user.Avatar
        )
        : UserRecord.Default;
        return record;
    }
    
    public virtual UserRecord GetUserData()
    {
        return UserRecord ?? UserRecord.Default;
    } 
    
    public virtual async Task<UserRecord> GetUserDataAsync()
    {
        UserRecord ??= await BuildUserDataAsync();
        return UserRecord;
    } 
    
    public virtual async Task<IdentityResult> UpdateUserAsync(UserRecord? newValues = null)
    {
        var user = await GetUserAsync(await AuthService.GetClaims());
        if (user is null) 
            return IdentityResult.Failed([new IdentityError { Description = "User not found." }]);
        
        if (newValues is not null)
        {
            user.DisplayName = newValues.DisplayName ?? user.DisplayName;
            user.Bio = newValues.Bio ?? user.Bio;
            if (newValues.AvatarBytes is not null && newValues.AvatarBytes.Length == 0)
            {
                user.Avatar = null;
            }
            else
            {
                user.Avatar = newValues.AvatarBytes ?? user.Avatar;
            }
        }
        
        var iResult = await UserManager.UpdateAsync(user);

        if (iResult.Succeeded)
            await RebuildAndNotify(true);
        
        return iResult;
    }
    
    public virtual async Task<IdentityResult> ChangeUsernameAsync(string username, string password)
    {
        var user = await GetUserAsync(await AuthService.GetClaims());
        if (user is null) 
            return IdentityResult.Failed([new IdentityError { Description = "User not found." }]);

        var res = await UserManager.CheckPasswordAsync(user, password);
        if (!res) return IdentityResult.Failed([new IdentityError { Description = "Password was incorrect." }]);
        
        var iResult = await UserManager.SetUserNameAsync(user, username);
        if (!iResult.Succeeded) return iResult;
        
        iResult = await UserManager.UpdateAsync(user);
        if (!iResult.Succeeded) return iResult;

        iResult = await UserManager.UpdateSecurityStampAsync(user);
        return iResult;
    }
    
    public virtual async Task<IdentityResult> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var user = await GetUserAsync(await AuthService.GetClaims());
        if (user is null) 
            return IdentityResult.Failed([new IdentityError { Description = "User not found." }]);
        
        var iResult = await UserManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!iResult.Succeeded) return iResult;
        
        iResult = await UserManager.UpdateAsync(user);
        if (!iResult.Succeeded) return iResult;

        iResult = await UserManager.UpdateSecurityStampAsync(user);
        return iResult;
    }
    
    public virtual async Task<IdentityResult> DeleteAccountAsync(string password)
    {
        var user = await GetUserAsync(await AuthService.GetClaims());
        if (user is null) 
            return IdentityResult.Failed([new IdentityError { Description = "User not found." }]);

        var res = await UserManager.CheckPasswordAsync(user, password);
        if (!res) return IdentityResult.Failed([new IdentityError { Description = "Password was incorrect." }]);

        var iResult = await UserManager.DeleteAsync(user);
        
        return iResult;
    }
    
    public virtual async Task Initialize()
    {
        await BuildUserDataAsync();
    }
    
    public virtual async Task RebuildAndNotify(bool invalidate = false)
    {
        if (invalidate)
            Invalidate();
        await BuildUserDataAsync();
        await NotifyAllAsync();
    }
    
    public virtual void Invalidate()
    {
        AuthService.Invalidate();
        UserRecord = null;
    }
    
    public List<IAsyncSubscriber> Subscribers { get; } = [];
    public bool AddSubscriber(IAsyncSubscriber subscriber)
    {
        if (Subscribers.Contains(subscriber)) return false;
        Subscribers.Add(subscriber);
        
        subscriber.OnUpdate();
        
        return true;
    }

    public bool RemoveSubscriber(IAsyncSubscriber subscriber)
    {
        return Subscribers.Remove(subscriber);
    }

    public async Task NotifyAllAsync()
    {
        foreach (var sub in Subscribers)
        {
            await sub.OnUpdate();
        }
    }
}

public interface IUserService : IAsyncPublisher
{
    /// <summary>
    /// Retrieval method for the User's display variables for classes utilizing this service.
    /// </summary>
    /// <returns>The User's display record, or a default display record if the UserRecord is not set.</returns>
    UserRecord GetUserData();
    
    /// <summary>
    /// Retrieval method for the User's display variables for classes utilizing this service.
    /// If the User Record does not currently exist, it is built for this request.
    /// </summary>
    /// <returns>The User's display record, or a default display record if the user is not found.</returns>
    Task<UserRecord> GetUserDataAsync();

    /// <summary>
    /// Update the user's representation in the database.
    /// </summary>
    /// <param name="newValues">
    /// The values, if any, of the user to be updated.
    /// </param>
    Task<IdentityResult> UpdateUserAsync(UserRecord? newValues = null);

    /// <summary>
    /// Update the user's username in the database, and updates (invalidate) the security stamp for the user.
    /// </summary>
    /// <param name="username">
    /// The inputted new username.
    /// </param>
    /// <param name="password">
    /// The inputted password that should match the user's current password.
    /// </param>
    /// <returns>
    /// An IdentityResult, indicating either success, or an error with a description of the issue.
    /// </returns>
    Task<IdentityResult> ChangeUsernameAsync(string username, string password);

    /// <summary>
    /// Update the user's password in the database, and updates (invalidates) the security stamp for the user.
    /// </summary>
    /// <param name="currentPassword">
    /// The inputted password that should match the user's current password.
    /// </param>
    /// <param name="newPassword">
    /// The inputted new password.
    /// </param>
    /// <returns>
    /// An IdentityResult, indicating either success, or an error with a description of the issue.
    /// </returns>
    Task<IdentityResult> ChangePasswordAsync(string currentPassword, string newPassword);

    /// <summary>
    /// Deletes the user's account.
    /// </summary>
    /// <param name="password">
    /// The inputted password that should match the user's current password.
    /// </param>
    /// <returns>
    /// An IdentityResult, indicating either success, or an error with a description of the issue.
    /// </returns>
    public Task<IdentityResult> DeleteAccountAsync(string password);
    
    /// <summary>
    /// Calls to initialize the User Record for retrieval by subscribers
    /// </summary>>
    Task Initialize();

    /// <summary>
    /// Invalidates the AuthService and the Record used for displaying attributes, then rebuilds from new data and notifies listeners.
    /// </summary>>
    Task RebuildAndNotify(bool invalidate = false);
    
    /// <summary>
    /// Invalidates the AuthService and the Record used for displaying attributes, Identity and the Record used for displaying attributes
    /// </summary>>
    void Invalidate();
}