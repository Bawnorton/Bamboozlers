using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Func;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.Services.UserServices;

public class AuthService : IAuthService
{
    public AuthService(AuthenticationStateProvider authenticationStateProvider,
        IHttpContextAccessor httpContextAccessor,
        IDbContextFactory<AppDbContext.AppDbContext> dbContextFactory)
    {
        AuthenticationStateProvider = authenticationStateProvider;
        HttpContextAccessor = httpContextAccessor;
        DbContextFactory = dbContextFactory;
        AuthenticationStateProvider.AuthenticationStateChanged += _ => Invalidate();
    }

    public AuthenticationStateProvider AuthenticationStateProvider { get; }
    public IDbContextFactory<AppDbContext.AppDbContext> DbContextFactory { get; }
    public IHttpContextAccessor HttpContextAccessor { get; }
    private ClaimsPrincipal? UserClaims { get; set; }
    private IIdentity? Identity { get; set; }
    
    private Cookie? Cookie { get; set; }

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
    
    public virtual Task<Cookie> GetCookie()
    {
        if (Cookie is not null) return Task.FromResult(Cookie);

        var httpContext = HttpContextAccessor.HttpContext;
        if (httpContext is null) throw new Exception("HttpContext is null");

        var cookieString = httpContext.Request.Cookies[".AspNetCore.Identity.Application"];
        if (cookieString is null) throw new Exception("Cookie is null");
        
        Cookie = new Cookie(".AspNetCore.Identity.Application", cookieString)
        {
            Domain = httpContext.Request.Host.Host
        };
        return Task.FromResult(Cookie);
    }

    public void Invalidate()
    {
        Identity = null;
        UserClaims = null;
    }

    public bool HasClaims()
    {
        return UserClaims is not null;
    }

    public bool HasIdentity()
    {
        return Identity is not null;
    }
}

public interface IAuthService
{
    /// <returns>
    ///     A boolean indicating whether the user is authenticated.
    /// </returns>
    public Task<bool> IsAuthenticated();

    /// <returns>
    ///     The current user, in strict database representation.
    /// </returns>
    /// <param name="inclusionCallback">
    ///     What data to include in the query.
    /// </param>
    /// <exception cref="Exception">
    ///     If <see cref="IsAuthenticated" /> returns false.
    /// </exception>
    Task<User?> GetUser(Unary<IQueryable<User>>? inclusionCallback = null);

    /// <returns>
    ///     The identity of the current user, or null if the user is not authenticated.
    /// </returns>
    Task<IIdentity?> GetIdentity();

    /// <returns>
    ///     The claims principal that describes the current user.
    /// </returns>
    Task<ClaimsPrincipal> GetClaims();

    /// <returns>
    ///     The authentication cookie of the current user.
    /// </returns>
    Task<Cookie> GetCookie();
    
    /// <summary>
    ///     Invalidates the currently cached User Claims, Identity and the Record used for displaying attributes
    /// </summary>
    /// >
    void Invalidate();
}