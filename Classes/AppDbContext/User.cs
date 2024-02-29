using Microsoft.AspNetCore.Identity;

namespace Bamboozlers.Classes.AppDbContext;

public class User:IdentityUser<int>
{
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public byte[]? Avatar { get; set; }
    public ICollection<Chat> Chats { get; set; }
    public ICollection<GroupChat> ModeratedChats { get; set; }
    public ICollection<GroupChat> OwnedChats { get; set; }
    // public ICollection<Block>? BlockedUsers { get; set; }
    // public ICollection<Block>? BlockedBy { get; set; }
}