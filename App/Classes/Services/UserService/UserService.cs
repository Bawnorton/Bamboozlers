using System.Security.Claims;
using System.Security.Principal;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Func;
using Bamboozlers.Classes.Utility.Observer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.Services.UserService;

public class UserService : IUserService
{
    private bool Initialized { get; set; }
    public AuthenticationStateProvider AuthenticationStateProvider { get; }
    public IDbContextFactory<AppDbContext.AppDbContext> DbContextFactory { get; }
    public UserManager<User> UserManager { get; }

    public UserService(AuthenticationStateProvider authenticationStateProvider,
        IDbContextFactory<AppDbContext.AppDbContext> dbContextFactory,
        UserManager<User> userManager)
    {
        AuthenticationStateProvider = authenticationStateProvider;
        DbContextFactory = dbContextFactory;
        UserManager = userManager;
        AuthenticationStateProvider.AuthenticationStateChanged += async _ => await Invalidate();
    }
    
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
    private Task<User?> GetUserAsync(ClaimsPrincipal claimsPrincipal)
    {
        return UserManager.GetUserAsync(claimsPrincipal);
    }

    /// <summary>
    /// Builds a display record corresponding to the current user.
    /// </summary>
    private async Task BuildUserDataAsync()
    {
        var user = await GetUserAsync(await GetClaims());
        
        UserRecord = user is not null
        ? new UserRecord(
            user.Id,
            user.UserName,
            user.Email,
            user.DisplayName,
            user.Bio,
            user.Avatar
        )
        : UserRecord.Default;
        
        NotifyAll();
    }
    
    public UserRecord GetUserData()
    {
        return UserRecord ?? UserRecord.Default;
    } 
    
    /* User Management */
    
    public async Task<IdentityResult> UpdateUserAsync(UserRecord? newValues = null)
    {
        var user = await GetUserAsync(await GetClaims());
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
        
        await Invalidate();
        
        return iResult;
    }
    
    public async Task<IdentityResult> ChangeUsernameAsync(string username, string password)
    {
        var user = await GetUserAsync(await GetClaims());
        if (user is null) 
            return IdentityResult.Failed([new IdentityError { Description = "User not found." }]);

        var res = await UserManager.CheckPasswordAsync(user, password);
        if (!res) return IdentityResult.Failed([new IdentityError { Description = "Password was incorrect." }]);
        
        var iResult = await UserManager.SetUserNameAsync(user, username);
        if (!iResult.Succeeded) return iResult;
        
        iResult = await UserManager.UpdateAsync(user);
        if (!iResult.Succeeded) return iResult;

        iResult = await UserManager.UpdateSecurityStampAsync(user);
        await Invalidate();
        return iResult;
    }
    
    public async Task<IdentityResult> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var user = await GetUserAsync(await GetClaims());
        if (user is null) 
            return IdentityResult.Failed([new IdentityError { Description = "User not found." }]);
        
        var iResult = await UserManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!iResult.Succeeded) return iResult;
        
        iResult = await UserManager.UpdateAsync(user);
        if (!iResult.Succeeded) return iResult;

        iResult = await UserManager.UpdateSecurityStampAsync(user);
        await Invalidate();
        return iResult;
    }
    
    public async Task<IdentityResult> DeleteAccountAsync(string password)
    {
        var user = await GetUserAsync(await GetClaims());
        if (user is null) 
            return IdentityResult.Failed([new IdentityError { Description = "User not found." }]);

        var res = await UserManager.CheckPasswordAsync(user, password);
        if (!res) return IdentityResult.Failed([new IdentityError { Description = "Password was incorrect." }]);

        var iResult = await UserManager.DeleteAsync(user);
        
        return iResult;
    }
    
    /* User Authentication, Retrieval and State */
    private ClaimsPrincipal? UserClaims { get; set; }
    private IIdentity? Identity { get; set; }
    
    public async Task<User?> GetUser(Unary<IQueryable<User>>? inclusionCallback = null)
    {
        if (!await IsAuthenticated()) 
            throw new Exception("User is not authenticated");
        
        await using var db = await DbContextFactory.CreateDbContextAsync();
        var query = db.Users.AsQueryable();
        query = inclusionCallback?.Invoke(query) ?? query;
        var user = await query.FirstOrDefaultAsync(u => u.UserName == Identity!.Name);
        
        await db.DisposeAsync();
        
        return user;
    }
    
    public async Task<bool> IsAuthenticated()
    {
        return await GetIdentity() is { IsAuthenticated: true };
    }
    
    /// <returns>
    /// The identity of the current user, or null if the user is not authenticated.
    /// </returns>
    private async Task<IIdentity?> GetIdentity()
    {
        return Identity ??= (await GetClaims()).Identity;
    }

    /// <returns>
    /// The claims principal that describes the current user.
    /// </returns>
    private async Task<ClaimsPrincipal> GetClaims()
    {
        return UserClaims ??= (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User;
    }
    
    public async Task Initialize()
    {
        if (Initialized) return;
        Initialized = true;
        
        await BuildUserDataAsync();
    }
    
    public async Task Invalidate(bool reinitialize = true)
    {
        UserRecord = null;
        UserClaims = null;
        Identity = null;
        Initialized = false;
        if (reinitialize)
        {
            await Initialize(); 
        }
    }

    public List<ISubscriber> Subscribers { get; } = [];
    public void AddSubscriber(ISubscriber subscriber)
    {
        if (Subscribers.Contains(subscriber)) return;
        Subscribers.Add(subscriber);
    }

    public bool RemoveSubscriber(ISubscriber subscriber)
    {
        return Subscribers.Remove(subscriber);
    }

    public void NotifyAll()
    {
        Subscribers.ForEach(x => x.OnUpdate());
    }
}