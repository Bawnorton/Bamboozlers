using System.Diagnostics.CodeAnalysis;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Utility.Observer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.Services.UserServices;

public class UserService(IAuthService authService, 
    ServiceProviderWrapper serviceProvider, 
    IDbContextFactory<AppDbContext.AppDbContext> dbContextFactory,
    IUserGroupService userGroupService) : IUserService
{
    private IAuthService AuthService { get; } = authService;
    private ServiceProviderWrapper ServiceProvider { get; } = serviceProvider;
    private IDbContextFactory<AppDbContext.AppDbContext> DbContextFactory { get; } = dbContextFactory;

    private IUserGroupService UserGroupService { get; } = userGroupService;

    /* User (Data) Retrieval */
    private UserRecord? UserRecord { get; set; }

    /// <summary>
    /// Builds a display record corresponding to the current user.
    /// </summary>
    private async Task<UserRecord> BuildUserDataAsync()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.GetUserAsync(await AuthService.GetClaims()).ConfigureAwait(false);
        
        var record = user is not null
        ? new UserRecord(
            user.Id,
            user.UserName,
            user.Email,
            user.DisplayName,
            user.Bio,
            user.Avatar,
            user.Deleted
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
        using var scope = ServiceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.GetUserAsync(await AuthService.GetClaims());

        if (user is null)
        {
            return IdentityResult.Failed([new IdentityError { Description = "User not found." }]);
        }

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

        var iResult = await userManager.UpdateAsync(user);

        if (iResult.Succeeded)
            await RebuildAndNotify(true);

        return iResult;
    }

    public virtual async Task<IdentityResult> ChangeUsernameAsync(string username, string password)
    {
        using var scope = ServiceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetService<UserManager<User>>()!;
        var user = await userManager.GetUserAsync(await AuthService.GetClaims());

        if (user is null)
            return IdentityResult.Failed([new IdentityError { Description = "User not found." }]);

        var res = await userManager.CheckPasswordAsync(user, password);
        if (!res) return IdentityResult.Failed([new IdentityError { Description = "Password was incorrect." }]);

        var iResult = await userManager.SetUserNameAsync(user, username);
        if (!iResult.Succeeded) return iResult;

        iResult = await userManager.UpdateAsync(user);
        if (!iResult.Succeeded) return iResult;

        iResult = await userManager.UpdateSecurityStampAsync(user);
        return iResult;
    }

    public virtual async Task<IdentityResult> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        using var scope = ServiceProvider.CreateScope();
        var userManager = ServiceProvider.GetService<UserManager<User>>()!;
        var user = await userManager.GetUserAsync(await AuthService.GetClaims());

        if (user is null)
            return IdentityResult.Failed([new IdentityError { Description = "User not found." }]);

        var iResult = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!iResult.Succeeded) return iResult;

        iResult = await userManager.UpdateAsync(user);
        if (!iResult.Succeeded) return iResult;

        iResult = await userManager.UpdateSecurityStampAsync(user);
        return iResult;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public virtual async Task<IdentityResult> DeleteAccountAsync(string password)
    {
        using var scope = ServiceProvider.CreateScope();
        var userManager = ServiceProvider.GetService<UserManager<User>>()!;
        var user = await userManager.GetUserAsync(await AuthService.GetClaims());

        if (user is null)
            return IdentityResult.Failed([new IdentityError { Description = "User not found." }]);

        var res = await userManager.CheckPasswordAsync(user, password);
        if (!res) return IdentityResult.Failed([new IdentityError { Description = "Password was incorrect." }]);

        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
    
        var gi = dbContext.GroupInvites.Where(i => i.SenderID == user.Id || i.RecipientID == user.Id).ToList();
        var fr = dbContext.FriendRequests.Where(fr => fr.SenderID == user.Id || fr.ReceiverID == user.Id).ToList();
        var b = dbContext.BlockList.Where(b => b.BlockerID == user.Id || b.BlockedID == user.Id).ToList();
        var f = dbContext.FriendShips.Where(f => f.User1ID == user.Id || f.User2ID == user.Id).ToList();
        var cm = dbContext.ChatModerators.Where(cm => cm.UserId == user.Id).ToList();
        var gc = dbContext.GroupChats.Where(gc => gc.OwnerID == user.Id).ToList();
        
        dbContext.GroupInvites.RemoveRange(gi);
        dbContext.FriendRequests.RemoveRange(fr);
        dbContext.BlockList.RemoveRange(b);
        dbContext.FriendShips.RemoveRange(f);
        dbContext.ChatModerators.RemoveRange(cm);

        user.Deleted = true;
        user.Email = "deleted@bamboozlers.com";
        user.UserName = "deleteduser" + user.Id;
        user.PasswordHash = null;
        user.SecurityStamp = "deleted";
        user.DisplayName = "Deleted User";
        user.Bio = null;
        user.Avatar = null;
        
        await dbContext.SaveChangesAsync();
        
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded) return result;
        
        gc.ForEach(_gc =>
            UserGroupService.FindSuccessorOwner(_gc.ID)
        );
        
        return result;
    }

    public virtual async Task Initialize()
    {
        await BuildUserDataAsync();
    }

    public virtual async Task RebuildAndNotify(bool invalidate = false)
    {
        if (invalidate) Invalidate();
        UserRecord = await BuildUserDataAsync();
        ((IPublisher<IUserSubscriber>) this).NotifyAll();
    }

    public virtual void Invalidate()
    {
        AuthService.Invalidate();
        UserRecord = null;
    }
    
    public List<IUserSubscriber> Subscribers { get; } = [];
    
    public bool AddSubscriber(IUserSubscriber subscriber) 
    {
        if (Subscribers.Contains(subscriber)) return false;
        Subscribers.Add(subscriber);
        
        subscriber.OnUpdate(UserRecord);
        
        return true;
    }
    
    public void NotifyAll()
    {
        foreach (var sub in Subscribers)
        {
            sub.OnUpdate(UserRecord);
        }
    }
}

public interface IUserService : IPublisher<IUserSubscriber>
{
    /// <summary>
    ///     Retrieval method for the User's display variables for classes utilizing this service.
    /// </summary>
    /// <returns>The User's display record, or a default display record if the UserRecord is not set.</returns>
    UserRecord GetUserData();

    /// <summary>
    ///     Retrieval method for the User's display variables for classes utilizing this service.
    ///     If the User Record does not currently exist, it is built for this request.
    /// </summary>
    /// <returns>The User's display record, or a default display record if the user is not found.</returns>
    Task<UserRecord> GetUserDataAsync();

    /// <summary>
    ///     Update the user's representation in the database.
    /// </summary>
    /// <param name="newValues">
    ///     The values, if any, of the user to be updated.
    /// </param>
    Task<IdentityResult> UpdateUserAsync(UserRecord? newValues = null);

    /// <summary>
    ///     Update the user's username in the database, and updates (invalidate) the security stamp for the user.
    /// </summary>
    /// <param name="username">
    ///     The inputted new username.
    /// </param>
    /// <param name="password">
    ///     The inputted password that should match the user's current password.
    /// </param>
    /// <returns>
    ///     An IdentityResult, indicating either success, or an error with a description of the issue.
    /// </returns>
    Task<IdentityResult> ChangeUsernameAsync(string username, string password);

    /// <summary>
    ///     Update the user's password in the database, and updates (invalidates) the security stamp for the user.
    /// </summary>
    /// <param name="currentPassword">
    ///     The inputted password that should match the user's current password.
    /// </param>
    /// <param name="newPassword">
    ///     The inputted new password.
    /// </param>
    /// <returns>
    ///     An IdentityResult, indicating either success, or an error with a description of the issue.
    /// </returns>
    Task<IdentityResult> ChangePasswordAsync(string currentPassword, string newPassword);

    /// <summary>
    ///     Deletes the user's account.
    /// </summary>
    /// <param name="password">
    ///     The inputted password that should match the user's current password.
    /// </param>
    /// <returns>
    ///     An IdentityResult, indicating either success, or an error with a description of the issue.
    /// </returns>
    Task<IdentityResult> DeleteAccountAsync(string password);

    /// <summary>
    ///     Calls to initialize the User Record for retrieval by subscribers
    /// </summary>
    /// >
    Task Initialize();

    /// <summary>
    ///     Invalidates the AuthService and the Record used for displaying attributes, then rebuilds from new data and notifies
    ///     listeners.
    /// </summary>
    /// >
    Task RebuildAndNotify(bool invalidate = false);

    /// <summary>
    ///     Invalidates the AuthService and the Record used for displaying attributes, Identity and the Record used for
    ///     displaying attributes
    /// </summary>
    /// >
    void Invalidate();
}