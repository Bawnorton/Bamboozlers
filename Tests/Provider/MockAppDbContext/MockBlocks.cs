using System.Linq.Expressions;
using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Tests.Provider.MockAppDbContext;

public class MockBlocks : AbstractMockDbSet<Block>
{
    private Mock<DbSet<Block>> mockBlocks;
    private readonly Func<Block, Block, bool> matchFunction =
        (b0, b1) => b0.BlockedID == b1.BlockedID && b0.BlockerID == b1.BlockerID;

    public MockBlocks(MockAppDbContext mockAppDbContext, DbSet<User> users) : base(mockAppDbContext)
    {
        var user2 = users.Skip(1).First();
        var user3 = users.Last();

        mockBlocks = MockAppDbContext.SetupMockDbSet(new List<Block>
        {
            new(user2.Id,user3.Id)
            {
                Blocked = user2,
                Blocker = user3,
            }
        });

        MockAppDbContext.MockDbContext.Setup(x => x.BlockList).Returns(mockBlocks.Object);
    }
    
    public override void AddMock(Block block)
    {
        mockBlocks = base.AddMock(
            block,
            mockBlocks,
            matchFunction
        );
        MockAppDbContext.MockDbContext.Setup(x => x.BlockList).Returns(mockBlocks.Object);
    }
    
    public override void RemoveMock(Block block)
    {
        mockBlocks = base.RemoveMock(
            block,
            mockBlocks,
            matchFunction
        );
        MockAppDbContext.MockDbContext.Setup(x => x.BlockList).Returns(mockBlocks.Object);
    }

    public override void UpdateMock(Block block)
    {
        RemoveMock(block);
        AddMock(block);
    }
    
    public override Block? FindMock(int idx)
    {
        return mockBlocks.Object.Skip(idx - 1).FirstOrDefault();
    }
    
    public override void ClearAll()
    {
        var list = mockBlocks.Object.ToList();
        foreach (var block in list)
        {
            RemoveMock(block);
        }
    }
}