using Microsoft.AspNetCore.Identity;

namespace Bamboozlers.Classes.AppDbContext;

public class User:IdentityUser<int>
{
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    
    //TODO: Migrate to cdn 
    public byte[]? Avatar { get; set; }
    public ICollection<Chat> Chats { get; set; }
    public ICollection<GroupChat> ModeratedChats { get; set; }
    public ICollection<GroupChat> OwnedChats { get; set; }
}