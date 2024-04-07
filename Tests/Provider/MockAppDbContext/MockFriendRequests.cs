using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Tests.Provider.MockAppDbContext;

public class MockFriendRequests : AbstractMockDbSet<FriendRequest>
{
    protected override Func<FriendRequest, FriendRequest, bool> MatchPredicate { get; set; } = (fr0, fr1) =>
        fr0.SenderID == fr1.SenderID && fr0.ReceiverID == fr1.ReceiverID;

    public MockFriendRequests(MockAppDbContext mockAppDbContext, DbSet<User> users) : base(mockAppDbContext)
    {
        var user1 = users.First();
        var user3 = users.Skip(2).First();
        var user5 = users.Skip(4).First();
        
        MockDbSet = MockAppDbContext.SetupMockDbSet(new List<FriendRequest>
        {
            new(user1.Id,user3.Id)
            {
                Sender = user1,
                Receiver = user3
            },
            new(user5.Id,user1.Id)
            {
                Sender = user5,
                Receiver = user1
            }
        });
    }
    
    public override void RebindMocks()
    {
        MockAppDbContext.MockDbContext.Setup(x => x.FriendRequests).Returns(GetMocks());
    }
}