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

    private static IIdentity? _identity;
    private static User? _self;

    public static void Init(AuthenticationStateProvider authStateProvider, IDbContextFactory<AppDbContext.AppDbContext> db)
    {
        _authStateProvider = authStateProvider;
        _db = db;
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
        if (_self is not null) return _self;
        
        _identity = await GetIdentity();
        if (_identity is { IsAuthenticated: true })
        {
            await using var db = await _db.CreateDbContextAsync();
            var query = db.Users.AsQueryable();
            query = inclusionCallback?.Invoke(query) ?? query;
            _self = await query.FirstOrDefaultAsync(u => u.UserName == _identity.Name);
        }
        else
        {
            throw new Exception("User is not authenticated");
        }

        return _self;
    }

    public static async Task<bool> IsAuthenticated()
    {
        return await GetIdentity() is { IsAuthenticated: true };
    }
    
    /// <returns>
    /// The identity of the current user, or null if the user is not authenticated.
    /// </returns>
    private static async Task<IIdentity?> GetIdentity()
    {
        return _identity ?? (await _authStateProvider.GetAuthenticationStateAsync()).User.Identity;
    }

    public static void InvalidateAuthState()
    {
        _identity = null;
        _self = null;
    }
}