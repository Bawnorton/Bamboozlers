using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.AppDbContext;

[PrimaryKey(nameof(SenderID), nameof(ReceiverID))]
public class FriendRequest
{
    public int SenderID { get; set; }
    public User Sender { get; set; }
    public int ReceiverID { get; set; }
    public User Receiver { get; set; }
    public RequestStatus Status { get; set; }
}
public enum RequestStatus
{
    Pending,
    Accepted,
    Denied
}