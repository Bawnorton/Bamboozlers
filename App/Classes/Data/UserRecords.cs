namespace Bamboozlers.Classes.Data;

/// <summary>
/// Record that stores the current user's display details. Used to form a model of the user to be displayed without directly interfering with its database representation.
/// Use of UserDisplayRecord avoids EntityFramework tracking, which can cause exceptions and interfere when changing the User's database representation.
/// </summary>

public record UserRecord(int? Id, string? UserName, string? Email, string? DisplayName, string? Bio, byte[]? AvatarBytes)
{
    public static UserRecord Default { get; } = new UserRecord(0, "N/A", "N/A", "N/A", "N/A", null);
    public string Avatar => GetDisplayableAvatar(AvatarBytes);

    /// <summary>
    /// Converts a raw byte array of data into a Base64 encoded string for image display.
    /// </summary>
    /// <param name="image">The image to be converted.</param>
    /// <returns>The encoded image string or, if image is null, the default avatar.</returns>
    private static string GetDisplayableAvatar(byte[]? image)
    {
        return image is null ? "images/default_profile.png" : $"data:image/png;base64,{Convert.ToBase64String(image)}";
    }

    public static UserRecord From(User user)
    {
        return new UserRecord(
            user.Id,
            user.UserName,
            user.Email,
            user.DisplayName,
            user.Bio,
            user.Avatar
        );
    }
    
    public UserRecord() : this(null, null, null, null, null, null) {}
}

/// <summary>
/// Enum for what User data has been changed (see UserDataRecord.cs)
/// </summary>
public enum UserDataType { Password, Username, Deletion, Email, Visual }

/// <summary>
/// Record type used to pass changes in user data between components.
/// </summary>
/// <param name="UserName">If set, the username value to pass.</param>
/// <param name="Email">If set, the email value to pass.</param>
/// <param name="DisplayName">If set, the display name value to pass.</param>
/// <param name="Bio">If set, the bio (or description) value to pass.</param>
/// <param name="Avatar">If set, the avatar value to pass.</param>
public record UserDataRecord(int? Id, string? UserName, string? Email, string? DisplayName, string? Bio, byte[]? AvatarBytes) : UserRecord
{
    public UserDataType? DataType { get; init; }
    public string? CurrentPassword { get; init; }
    public string? NewPassword { get; init; }
    
    public UserDataRecord() : this(null, null, null, null, null, null) {}
}

/// <summary>
/// Record used to pass information about attempted User changes.
/// </summary>
/// <param name="DataType">The User attribute that was changed. (Visual category for non-login details)</param>
/// <param name="Success">If the action was successful.</param>
/// <param name="Reason">The error message, if any.</param>
public record UserUpdateResult(UserDataType DataType, bool Success, string Reason);

/// <summary>
/// Record used to store ONLY User visual data.
/// </summary>
/// <param name="UserName">The user's username.</param>
/// <param name="DisplayName">The user's username, if any.</param>
/// <param name="Avatar">The user's avatar, if any.</param>
public record UserVisualRecord(string UserName, string? DisplayName, byte[]? Avatar);
