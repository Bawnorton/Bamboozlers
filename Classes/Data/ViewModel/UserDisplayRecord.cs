using System.Diagnostics;
using Bamboozlers.Classes.AppDbContext;

namespace Bamboozlers.Classes.Data.ViewModel;

/// <summary>
/// Record that stores the current user's display details. Used to form a model of the user to be displayed without directly interfering with its database representation.
/// Use of UserDisplayRecord avoids EntityFramework tracking, which can cause exceptions and interfere when changing the User's database representation.
/// </summary>

public record UserDisplayRecord
{
    public static string? UserName { get; set; }
    public static string? Email { get; set; }
    public static string? DisplayName { get; set; }
    public static string? Bio { get; set; }
    public static string? Avatar { get; set; }

    /// <summary>
    /// Converts a raw byte array of data into a Base64 encoded string for image display.
    /// </summary>
    /// <param name="image">The image to be converted.</param>
    /// <returns>The encoded image string or, if image is null, the default avatar.</returns>
    public static string GetDisplayableAvatar(byte[]? image)
    {
        return image is null ? "images/default_profile.png" : $"data:image/png;base64,{Convert.ToBase64String(image)}";
    }
    
    /// <summary>
    /// Updates the static attributes of the display record given a User object.
    /// </summary>
    /// <param name="user">The user from which attributes will be set.</param>
    public static void Update(User? user)
    {
        if (user is null)
            return;
        
        UserName = user.UserName ?? UserName;
        Email = user.Email ?? Email;
        DisplayName = user.DisplayName;
        Bio = user.Bio;
        Avatar = GetDisplayableAvatar(user.Avatar);
    }
}