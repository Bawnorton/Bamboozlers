using System.Transactions;
using Bamboozlers.Classes.AppDbContext;

namespace Bamboozlers.Classes;

public enum UserDataType { Password, Username, Deletion, Email, Visual }

public record UserDataRecord(string? UserName, string? Email, string? DisplayName, string? Bio, byte[]? Avatar)
{
    public UserDataType? DataType { get; init; }
    public string? CurrentPassword { get; init; }
    public string? NewPassword { get; init; }
    
    public UserDataRecord() : this(null, null, null, null, null) {}
}

public record UserDisplayRecord
{
    public static string? UserName { get; set; }
    public static string? Email { get; set; }
    public static string? DisplayName { get; set; }
    public static string? Bio { get; set; }
    public static string? Avatar { get; set; }

    private static string GetDisplayableAvatar(byte[]? value)
    {
        return value is null ? "images/default_profile.png" : $"data:image/png;base64,{Convert.ToBase64String(value)}";
    }
    
    public static void UpdateDisplayRecord(User user)
    {
        UserName = user.UserName;
        Email = user.Email;
        DisplayName = user.DisplayName;
        Bio = user.Bio;
        Avatar = GetDisplayableAvatar(user.Avatar);
    }
}