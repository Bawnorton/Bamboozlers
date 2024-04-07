using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Tests.Provider.MockAppDbContext;

public class MockBlocks : AbstractMockDbSet<Block>
{
    protected override Func<Block, Block, bool> MatchPredicate { get; set; } =
        (b0, b1) => b0.BlockedID == b1.BlockedID && b0.BlockerID == b1.BlockerID;

    public override void RebindMocks()
    {
        MockAppDbContext.MockDbContext.Setup(x => x.BlockList).Returns(GetMocks());
    }

    public MockBlocks(MockAppDbContext mockAppDbContext, DbSet<User> users) : base(mockAppDbContext)
    {
        var user2 = users.Skip(1).First();
        var user3 = users.Last();

        MockDbSet = MockAppDbContext.SetupMockDbSet(new List<Block>
        {
            new(user2.Id,user3.Id)
            {
                Blocked = user2,
                Blocker = user3,
            }
        });
    }
}