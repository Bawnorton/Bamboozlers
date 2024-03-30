using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.AppDbContext;

public class User:IdentityUser<int>
{
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public byte[]? Avatar { get; set; }
    public ICollection<Chat> Chats { get; set; }
    public ICollection<GroupChat> ModeratedChats { get; set; }
    public ICollection<GroupChat> OwnedChats { get; set; }

    public async Task<List<User>> GetFriends(AppDbContext db)
    {
        var Friends = new List<User>();
        var friendships = await db.FriendShips.Include(f => f.User1).Include(f => f.User2).ToListAsync();
        foreach (var friendship in friendships)
        {
            if (friendship.User1ID == Id)
            {
                Friends.Add(friendship.User2);
            }
            else if (friendship.User2ID == Id)
            {
                Friends.Add(friendship.User1);
            }
        }

        return Friends;
    }
}