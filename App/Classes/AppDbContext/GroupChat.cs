namespace Bamboozlers.Classes.AppDbContext;

public class GroupChat(int ownerID) : Chat
{
    public User Owner { get; set; }
    public int OwnerID { get; set; } = ownerID;
    public ICollection<User> Moderators { get; set; }
    public string? Name { get; set; }
    public byte[]? Avatar { get; set; }

    public string GetGroupName()
    {
        return Name ?? $"{Owner.GetName()}'s Group";
    }
}