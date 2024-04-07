using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.AppDbContext;

[PrimaryKey(nameof(SenderID), nameof(ReceiverID))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class FriendRequest(int SenderID, int ReceiverID)
{
    public int SenderID { get; set; } = SenderID;
    public User Sender { get; set; } = default!;
    public int ReceiverID { get; set; } = ReceiverID;
    public User Receiver { get; set; } = default!;
    public RequestStatus Status { get; set; }
}

public enum RequestStatus
{
    Pending,
    Denied
}