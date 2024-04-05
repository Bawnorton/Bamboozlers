using Blazorise.Extensions;

namespace Bamboozlers.Classes.AppDbContext;

public class GroupChat:Chat
{
    public GroupChat(int OwnerID)
    {
        this.OwnerID = OwnerID;
    }
    
    public User Owner { get; set; }
    public int OwnerID { get; set; }
    public ICollection<User> Moderators { get; set; }
    public string? Name { get; set; }
    public byte[]? Avatar { get; set; }

    public string GetGroupName()
    {
        if (Name is not null) return Name;
        
        return Owner is null ? "Group Chat" : $"{Owner.GetName()}'s Group";
    }
}