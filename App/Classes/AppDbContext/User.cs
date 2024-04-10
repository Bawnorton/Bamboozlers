using System.Diagnostics.CodeAnalysis;
using Blazorise.Extensions;
using Microsoft.AspNetCore.Identity;

namespace Bamboozlers.Classes.AppDbContext;

[SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
public class User : IdentityUser<int>
{
    public string? DisplayName { get; set; }
    public string? Bio { get; set; } 
    public byte[]? Avatar { get; set; }
    public ICollection<Chat> Chats { get; set; } = default!;
    public ICollection<GroupChat> ModeratedChats { get; set; } = default!;
    public ICollection<GroupChat> OwnedChats { get; set; } = default!;
    public ICollection<ChatUser> UserChats { get; set; } = default!;
    public ICollection<ChatModerator> UserModeratedChats { get; set; } = default!;

    public string GetName()
    {
        return DisplayName.IsNullOrEmpty() ? UserName! : DisplayName!;
    }

    public string GetAvatar()
    {
        return Avatar is null ? GetDefaultAvatar() : $"data:image/png;base64,{Convert.ToBase64String(Avatar)}";
    }

    public string GetDefaultAvatar()
    {
        return Id == -1 
            ? "images/default_profiles/profile_deleted.png" 
            : $"images/default_profiles/profile_{Id % 7}.png";
    }
    
    public static readonly User Deleted = new()
    {
        Avatar = null,
        UserName = "Deleted User",
        Id = -1
    };
}