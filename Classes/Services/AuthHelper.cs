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
    //private static User? _self;
    private static IIdentity? _identity;
    
    public static void Init(AuthenticationStateProvider authStateProvider, IDbContextFactory<AppDbContext.AppDbContext> db)
    {
        _authStateProvider = authStateProvider;
        _authStateProvider.AuthenticationStateChanged += Invalidate;
        _db = db;
    } 
    
    /// <returns>
    /// The current user.
    /// </returns>
    /// <param name="inculsionCallback">
    /// What data to include in the query.
    /// </param>
    /// <exception cref="Exception">
    /// If <see cref="IsAuthenticated"/> returns false.
    /// </exception>
    public static async Task<User> GetSelf(Unary<IQueryable<User>>? inculsionCallback = null)
    {
        //if (_self is not null) return _self;

        User self;
        var identity = GetIdentity();
        if (identity is { IsAuthenticated: true })
        {
            await using var db = await _db.CreateDbContextAsync();
            var query = db.Users.AsQueryable();
            query = inculsionCallback?.Invoke(query) ?? query;
            self = await query.FirstAsync(u => u.UserName == identity.Name);
        }
        else
        {
            throw new Exception("User is not authenticated");
        }

        return self;
    }

    public static bool IsAuthenticated()
    {
        return GetIdentity() is { IsAuthenticated: true };
    }
    
    /// <returns>
    /// The identity of the current user, or null if the user is not authenticated.
    /// </returns>
    private static IIdentity? GetIdentity()
    {
        if (_identity is not null) return _identity;
        
        var authState = _authStateProvider.GetAuthenticationStateAsync().Result;
        _identity = authState.User.Identity;
        return _identity;
    }

    /// <summary>
    /// Invalidate the previously cached identity of the user when changes to
    /// authentication state are made.
    /// </summary>
    /// <param name="authStateTask">
    /// Task wrapped with information about the user.
    /// </param>
    private static void Invalidate(Task<AuthenticationState> authStateTask)
    {
        _identity = null;
    }
}