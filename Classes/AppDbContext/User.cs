using System.ComponentModel.DataAnnotations;

namespace Bamboozlers.Classes.AppDbContext;

public class User
{
    [Key]
    public int ID { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    
    //TODO: Migrate to cdn 
    public byte[]? Avatar { get; set; }
    public ICollection<Chat> Chats { get; set; }
    public ICollection<GroupChat> ModeratedChats { get; set; }
    public ICollection<GroupChat> OwnedChats { get; set; }
}