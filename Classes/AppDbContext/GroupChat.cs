namespace Bamboozlers.Classes.AppDbContext;

public class GroupChat:Chat
{
    public User Owner { get; set; }
    public int OwnerID { get; set; }
    public ICollection<User> Moderators { get; set; }
    public string Name { get; set; }
    public byte[]? Avatar { get; set; }
}