using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.AppDbContext;

[PrimaryKey(nameof(SenderID), nameof(RecipientID), nameof(GroupID))]
public class GroupInvite
{
    public GroupInvite(int SenderID, int RecipientID, int GroupID)
    {
        this.SenderID = SenderID;
        this.RecipientID = RecipientID;
        this.GroupID = GroupID;
    }

    public int SenderID { get; set; }
    public User Sender { get; set; }
    public int RecipientID { get; set; }
    public User Recipient { get; set; }
    public GroupChat Group { get; set; }
    public int GroupID { get; set; }
    public RequestStatus Status { get; set; }
}