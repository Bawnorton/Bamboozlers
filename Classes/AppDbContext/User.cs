namespace Bamboozlers.Classes.AppDbContext;

public class User
{
    public int ID { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public byte[]? Avatar { get; set; }
    public ICollection<Chat> Chats { get; set; }
    public ICollection<GroupChat> ModeratedChats { get; set; }
    public ICollection<GroupChat> OwnedChats { get; set; }
    // public ICollection<Block>? BlockedUsers { get; set; }
    // public ICollection<Block>? BlockedBy { get; set; }
}