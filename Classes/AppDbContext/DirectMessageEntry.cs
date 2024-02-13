using System.ComponentModel.DataAnnotations;

namespace Bamboozlers.Classes.AppDbContext;

public class DirectMessageEntry
{
    [Key]
    public required int FriendshipId { get; set; }
    
    public required int FriendUserId { get; set; }

    public required int SelfUserId { get; set; }
}