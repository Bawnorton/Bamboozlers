using System.Linq.Expressions;
using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Tests.Provider.MockAppDbContext;

public class MockFriendships : AbstractMockDbSet<Friendship>
{
    public Mock<DbSet<Friendship>> mockFriendships;
    private readonly Func<Friendship, Friendship, bool> matchFunction = (f0, f1) =>
        (f0.User1ID == f1.User1ID && f0.User2ID == f1.User2ID) || (f0.User1ID == f1.User2ID && f0.User2ID == f1.User1ID);

    public MockFriendships(MockAppDbContext mockAppDbContext, DbSet<User> users) : base(mockAppDbContext)
    {
        var user0 = users.First();
        var user1 = users.Skip(1).First();
        var user2 = users.Skip(2).First();

        mockFriendships = MockAppDbContext.SetupMockDbSet(new List<Friendship>
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
        
        MockAppDbContext.MockDbContext.Setup(x => x.FriendShips).Returns(mockFriendships.Object);
    }
    
    public override void AddMock(Friendship friendship)
    {
        mockFriendships = base.AddMock(
            friendship,
            mockFriendships,
            matchFunction
        );
        MockAppDbContext.MockDbContext.Setup(x => x.FriendShips).Returns(mockFriendships.Object);
    }
    
    public override void RemoveMock(Friendship friendship)
    {
        mockFriendships = base.RemoveMock(
            friendship,
            mockFriendships,
            matchFunction
        );
        MockAppDbContext.MockDbContext.Setup(x => x.FriendShips).Returns(mockFriendships.Object);
    }

    public override void UpdateMock(Friendship friendship)
    {
        RemoveMock(friendship);
        AddMock(friendship);
    }
    
    public override Friendship? FindMock(int idx)
    {
        return mockFriendships.Object.Skip(idx - 1).FirstOrDefault();
    }
    
    public override void ClearAll()
    {
        var list = mockFriendships.Object.ToList();
        foreach (var friendship in list)
        {
            RemoveMock(friendship);
        }
    }
}