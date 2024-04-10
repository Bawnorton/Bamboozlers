using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.AppDbContext;

[PrimaryKey(nameof(UserId),nameof(GroupChatId))]
public class ChatModerator(int userId, int groupChatId)
{
    public User User { get; set; } = default!;
    public int UserId { get; set; } = userId;
    public GroupChat GroupChat { get; set; } = default!;
    public int GroupChatId { get; set; } = groupChatId;
}