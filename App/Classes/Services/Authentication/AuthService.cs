using System.Security.Claims;
using System.Security.Principal;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Func;
using Bamboozlers.Classes.Utility.Observer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Bamboozlers.Classes.Services.Authentication;

public class AuthService : IAuthService
{
    public AuthenticationStateProvider AuthenticationStateProvider { get; }
    public IDbContextFactory<AppDbContext.AppDbContext> DbContextFactory { get; }
    private ClaimsPrincipal? UserClaims { get; set; }
    private IIdentity? Identity { get; set; }

    public AuthService(AuthenticationStateProvider authenticationStateProvider, 
        IDbContextFactory<AppDbContext.AppDbContext> dbContextFactory)
    {
        AuthenticationStateProvider = authenticationStateProvider;
        DbContextFactory = dbContextFactory;
        AuthenticationStateProvider.AuthenticationStateChanged += _ => Invalidate();
    }
    
    public virtual async Task<User?> GetUser(Unary<IQueryable<User>>? inclusionCallback = null)
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
    
    public virtual async Task<bool> IsAuthenticated()
    {
        return await GetIdentity() is { IsAuthenticated: true };
    }
    
    public virtual async Task<IIdentity?> GetIdentity()
    {
        return Identity ??= (await GetClaims()).Identity;
    }
    
    public virtual async Task<ClaimsPrincipal> GetClaims()
    {
        UserClaims ??= (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User;
        return UserClaims;
    }

    public void Invalidate()
    {
        Identity = null;
        UserClaims = null;
    }

    public bool HasClaims() => UserClaims is not null;
    public bool HasIdentity() => Identity is not null;
}

public interface IAuthService
{
    /// <returns>
    /// A boolean indicating whether the user is authenticated.
    /// </returns>
    public Task<bool> IsAuthenticated();

    /// <returns>
    /// The current user, in strict database representation.
    /// </returns>
    /// <param name="inclusionCallback">
    /// What data to include in the query.
    /// </param>
    /// <exception cref="Exception">
    /// If <see cref="IsAuthenticated"/> returns false.
    /// </exception>
    Task<User?> GetUser(Unary<IQueryable<User>>? inclusionCallback = null);
    
    /// <returns>
    /// The identity of the current user, or null if the user is not authenticated.
    /// </returns>
    Task<IIdentity?> GetIdentity();
    
    /// <returns>
    /// The claims principal that describes the current user.
    /// </returns>
    Task<ClaimsPrincipal> GetClaims();
    
    /// <summary>
    /// Invalidates the currently cached User Claims, Identity and the Record used for displaying attributes
    /// </summary>>
    void Invalidate();
}