using Bamboozlers.Classes.AppDbContext;

namespace Bamboozlers.Classes;

public class ClientChat
{
    public Chat DbChat { get; set; }

    public int ID => DbChat.ID;
    
    public string Name { get; set; }
    
    public string? Avatar { get; set; }
    
    public bool IsGroupChat => DbChat is GroupChat;
    
    public bool IsDirectChat => !IsGroupChat;
}