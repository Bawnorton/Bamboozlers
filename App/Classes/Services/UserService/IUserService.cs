using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Func;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.Services.UserService;

public interface IUserService
{
    protected AuthenticationStateProvider AuthenticationStateProvider { get; }
    protected IDbContextFactory<AppDbContext.AppDbContext> DbContextFactory { get; }
    protected UserManager<User> UserManager { get; }
    public UserRecord UserRecord { get; } 
    
    /// <returns>
    /// Returns the current User's data in UserRecord form.
    /// </returns>
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
    public Task<User?> GetUser(Unary<IQueryable<User>>? inclusionCallback = null);
    
    /// <summary>
    /// Invalidates the currently cached User Claims, Identity and the Record used for displaying attributes
    /// </summary>>
    void Invalidate();
}