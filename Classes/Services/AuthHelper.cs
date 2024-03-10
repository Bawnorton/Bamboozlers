using System.Linq.Expressions;
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
        User? self;
        var identity = await GetIdentity();
        if (identity is { IsAuthenticated: true })
        {
            await using var db = await _db.CreateDbContextAsync();
            var query = db.Users.AsQueryable();
            query = inclusionCallback?.Invoke(query) ?? query;
            self = await query.FirstOrDefaultAsync(u => u.UserName == identity.Name);
        }
        else
        {
            throw new Exception("User is not authenticated");
        }

        return self;
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
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return authState.User.Identity;
    }
}