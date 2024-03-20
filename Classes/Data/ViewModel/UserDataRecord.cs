namespace Bamboozlers.Classes.Data.ViewModel;

/// <summary>
/// Record type used to pass changes in user data between components.
/// </summary>
/// <param name="UserName">If set, the username value to pass.</param>
/// <param name="Email">If set, the email value to pass.</param>
/// <param name="DisplayName">If set, the display name value to pass.</param>
/// <param name="Bio">If set, the bio (or description) value to pass.</param>
/// <param name="Avatar">If set, the avatar value to pass.</param>
public record UserDataRecord(int? Id, string? UserName, string? Email, string? DisplayName, string? Bio, byte[]? Avatar)
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