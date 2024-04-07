using System.Diagnostics.CodeAnalysis;

namespace Bamboozlers.Classes.AppDbContext;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
public class GroupChat(int OwnerID) : Chat
{
    public User Owner { get; set; } = default!;
    public int OwnerID { get; set; } = OwnerID;
    public ICollection<User> Moderators { get; set; } = default!;
    public string? Name { get; set; }
    public byte[]? Avatar { get; set; }

    public string GetGroupName()
    {
        return Name ?? $"{Owner.GetName()}'s Group";
    }

    public string GetGroupAvatar()
    {
        return Avatar is null ? GetDefaultAvatar() : $"data:image/png;base64,{Convert.ToBase64String(Avatar)}";
    }
    
    public string GetDefaultAvatar()
    {
        return $"images/default_groups/group_{ID % 7}.png";
    }
}