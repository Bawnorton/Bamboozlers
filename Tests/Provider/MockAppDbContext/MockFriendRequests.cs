using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Tests.Provider.MockAppDbContext;

public class MockFriendRequests : AbstractMockDbSet<FriendRequest>
{
    private Mock<DbSet<FriendRequest>> mockFriendRequests;
    private readonly Func<FriendRequest, FriendRequest, bool> matchFunction = (fr0, fr1) =>
        fr0.SenderID == fr1.SenderID && fr0.ReceiverID == fr1.ReceiverID;

    public MockFriendRequests(MockAppDbContext mockAppDbContext, DbSet<User> users) : base(mockAppDbContext)
    {
        var user1 = users.First();
        var user3 = users.Last();

        mockFriendRequests = MockAppDbContext.SetupMockDbSet(new List<FriendRequest>
        {
            new(user1.Id,user3.Id)
            {
                Sender = user1,
                Receiver = user3
            }
        });
        MockAppDbContext.MockDbContext.Setup(x => x.FriendRequests).Returns(mockFriendRequests.Object);
    }
    
    public override void AddMock(FriendRequest friendRequest)
    {
        mockFriendRequests = base.AddMock(
            friendRequest,
            mockFriendRequests,
            matchFunction
        );
        MockAppDbContext.MockDbContext.Setup(x => x.FriendRequests).Returns(mockFriendRequests.Object);
    }
    
    public override void RemoveMock(FriendRequest friendRequest)
    {
        mockFriendRequests = base.RemoveMock(
            friendRequest,
            mockFriendRequests,
            matchFunction
        );
        MockAppDbContext.MockDbContext.Setup(x => x.FriendRequests).Returns(mockFriendRequests.Object);
    }
    
    public override void UpdateMock(FriendRequest friendRequest)
    {
        RemoveMock(friendRequest);
        AddMock(friendRequest);
    }
    
    public override FriendRequest? FindMock(int idx)
    {
        return mockFriendRequests.Object.Skip(idx - 1).FirstOrDefault();
    }
    
    public override void ClearAll()
    {
        var list = mockFriendRequests.Object.ToList();
        foreach (var friendRequest in list)
        {
            RemoveMock(friendRequest);
        }
    }
}