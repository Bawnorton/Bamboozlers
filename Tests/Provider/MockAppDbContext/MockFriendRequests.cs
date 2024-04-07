using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Tests.Provider.MockAppDbContext;

public class MockFriendRequests : AbstractMockDbSet<FriendRequest>
{
    private readonly Func<FriendRequest, FriendRequest, bool> _matchFunction = (fr0, fr1) =>
        fr0.SenderID == fr1.SenderID && fr0.ReceiverID == fr1.ReceiverID;

    private Mock<DbSet<FriendRequest>> _mockFriendRequests;

    public MockFriendRequests(MockAppDbContext mockAppDbContext, IQueryable<User> users) : base(mockAppDbContext)
    {
        var user1 = users.First();
        var user3 = users.Last();

        _mockFriendRequests = MockAppDbContext.SetupMockDbSet(new List<FriendRequest>
        {
            new(user1.Id, user3.Id)
            {
                Sender = user1,
                Receiver = user3
            }
        });
        MockAppDbContext.MockDbContext.Setup(x => x.FriendRequests).Returns(_mockFriendRequests.Object);
    }

    public override void AddMock(FriendRequest friendRequest)
    {
        _mockFriendRequests = base.AddMock(
            friendRequest,
            _mockFriendRequests,
            _matchFunction
        );
        MockAppDbContext.MockDbContext.Setup(x => x.FriendRequests).Returns(_mockFriendRequests.Object);
    }

    public override void RemoveMock(FriendRequest friendRequest)
    {
        _mockFriendRequests = base.RemoveMock(
            friendRequest,
            _mockFriendRequests,
            _matchFunction
        );
        MockAppDbContext.MockDbContext.Setup(x => x.FriendRequests).Returns(_mockFriendRequests.Object);
    }

    public override void UpdateMock(FriendRequest friendRequest)
    {
        RemoveMock(friendRequest);
        AddMock(friendRequest);
    }

    public override FriendRequest? FindMock(int idx)
    {
        return _mockFriendRequests.Object.Skip(idx - 1).FirstOrDefault();
    }

    public override void ClearAll()
    {
        var list = _mockFriendRequests.Object.ToList();
        foreach (var friendRequest in list)
        {
            RemoveMock(friendRequest);
        }
    }
}