using System.Security.Claims;
using System.Security.Principal;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Func;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.Services;

public static class AuthHelper
{
    private static AuthenticationStateProvider _authStateProvider;
    private static IDbContextFactory<AppDbContext.AppDbContext> _db;

    private static User? _self;
    private static IIdentity? _identity;
    
    public static void Init(AuthenticationStateProvider authStateProvider, IDbContextFactory<AppDbContext.AppDbContext> db)
    {
        _authStateProvider = authStateProvider;
        _db = db;

        authStateProvider.AuthenticationStateChanged += _ => Invalidate();
    } 
    
    /// <returns>
    /// The current user.
    /// </returns>
    /// <param name="inclusionCallback">
    /// What data to include in the query.
    /// </param>
    /// <exception cref="Exception">
    /// If <see cref="IsAuthenticated"/> returns false.
    /// </exception>
    public static async Task<User?> GetSelf(Unary<IQueryable<User>>? inclusionCallback = null)
    {
        if (_self is not null) 
            return _self;
        
        _identity = await GetIdentity();
        if (_identity is not { IsAuthenticated: true }) 
            throw new Exception("User is not authenticated");
        
        await using var db = await _db.CreateDbContextAsync();
        var query = db.Users.AsQueryable();
        query = inclusionCallback?.Invoke(query) ?? query;
        _self = await query.FirstOrDefaultAsync(u => u.UserName == _identity.Name);
        
        await db.DisposeAsync();
        
        return _self;
    }

    public static async Task<bool> IsAuthenticated()
    {
        return await GetIdentity() is { IsAuthenticated: true };
    }
    
    /// <returns>
    /// The identity of the current user, or null if the user is not authenticated.
    /// </returns>
    public static async Task<IIdentity?> GetIdentity()
    {
        return _identity ?? (await GetClaims()).Identity;
    }

    /// <returns>
    /// The claims principal that describes the current user.
    /// </returns>
    private static async Task<ClaimsPrincipal> GetClaims()
    {
        return (await _authStateProvider.GetAuthenticationStateAsync()).User;
    }

    public static void Invalidate()
    {
        _self = null;
        _identity = null;
    }
}