using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Tests.Provider.MockAppDbContext;

public class MockFriendships : AbstractMockDbSet<Friendship>
{
    private readonly Func<Friendship, Friendship, bool> _matchFunction = (f0, f1) =>
        (f0.User1ID == f1.User1ID && f0.User2ID == f1.User2ID) ||
        (f0.User1ID == f1.User2ID && f0.User2ID == f1.User1ID);

    private Mock<DbSet<Friendship>> _mockFriendships;

    public MockFriendships(MockAppDbContext mockAppDbContext, IQueryable<User> users) : base(mockAppDbContext)
    {
        var user0 = users.First();
        var user1 = users.Skip(1).First();
        var user2 = users.Skip(2).First();

        _mockFriendships = MockAppDbContext.SetupMockDbSet(new List<Friendship>
        {
            new(user0.Id, user1.Id)
            {
                User1 = user0,
                User2 = user1
            },
            new(user1.Id, user2.Id)
            {
                User1 = user1,
                User2 = user2
            }
        });

        MockAppDbContext.MockDbContext.Setup(x => x.FriendShips).Returns(_mockFriendships.Object);
    }

    public override void AddMock(Friendship friendship)
    {
        _mockFriendships = base.AddMock(
            friendship,
            _mockFriendships,
            _matchFunction
        );
        MockAppDbContext.MockDbContext.Setup(x => x.FriendShips).Returns(_mockFriendships.Object);
    }

    public override void RemoveMock(Friendship friendship)
    {
        _mockFriendships = base.RemoveMock(
            friendship,
            _mockFriendships,
            _matchFunction
        );
        MockAppDbContext.MockDbContext.Setup(x => x.FriendShips).Returns(_mockFriendships.Object);
    }

    public override void UpdateMock(Friendship friendship)
    {
        RemoveMock(friendship);
        AddMock(friendship);
    }

    public override Friendship? FindMock(int idx)
    {
        return _mockFriendships.Object.Skip(idx - 1).FirstOrDefault();
    }

    public override void ClearAll()
    {
        var list = _mockFriendships.Object.ToList();
        foreach (var friendship in list)
        {
            RemoveMock(friendship);
        }
    }
}