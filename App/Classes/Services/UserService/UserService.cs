using System.Security.Claims;
using System.Security.Principal;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Func;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.Services.UserService;

public class UserService : IUserService
{
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
        
        AuthenticationStateProvider.AuthenticationStateChanged += _ => Invalidate();
    }
    
    /* User (Data) Retrieval */

    public UserRecord? UserRecord { get; private set; }
    
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
    /// Build record of current user.
    /// </summary>
    private async Task BuildUserDataAsync()
    {
        var user = await GetUserAsync(await GetClaims());
        if (user is null)
        {
            UserRecord = UserRecord.Default;
            return;
        }
        
        UserRecord = new UserRecord(
            user.Id, 
            user.UserName, 
            user.Email, 
            user.DisplayName, 
            user.Bio, 
            user.Avatar
        );
    }

    public async Task<UserRecord> GetUserDataAsync()
    {
        if (UserRecord is null)
            await BuildUserDataAsync();
        return UserRecord!;
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
            
            if (newValues.AvatarBytes is null)
            {
                user.Avatar = null;
            }
            else
            {
                user.Avatar = newValues.AvatarBytes ?? user.Avatar;
            }
        }
        
        var iResult = await UserManager.UpdateAsync(user);
        
        Invalidate();
        await BuildUserDataAsync();
        
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
        Invalidate();
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
        Invalidate();
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
    
    public void Invalidate()
    {
        UserRecord = null;
        UserClaims = null;
        Identity = null;
    }
}