using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Tests.Provider.MockAppDbContext;

public class MockFriendships : AbstractMockDbSet<Friendship>
{
    protected override Func<Friendship, Friendship, bool> MatchPredicate {get; set;} = (f0, f1) =>
        (f0.User1ID == f1.User1ID && f0.User2ID == f1.User2ID) || (f0.User1ID == f1.User2ID && f0.User2ID == f1.User1ID);

    public MockFriendships(MockAppDbContext mockAppDbContext, DbSet<User> users) : base(mockAppDbContext)
    {
        var user0 = users.First();
        var user1 = users.Skip(1).First();
        var user2 = users.Skip(2).First();

        MockDbSet = MockAppDbContext.SetupMockDbSet(new List<Friendship>
        {
            new(user0.Id,user1.Id)
            {
                User1 = user0,
                User2 = user1
            },
            new(user1.Id,user2.Id)
            {
                User1 = user1,
                User2 = user2
            },
        });
    }
    
    public override void RebindMocks()
    {
        MockAppDbContext.MockDbContext.Setup(x => x.FriendShips).Returns(GetMocks());
    }
}