using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Tests.Provider.MockAppDbContext;

public class MockUsers : AbstractMockDbSet<User>
{
    private readonly Func<User, User, bool> _matchFunction = (u0, u1) => u0.Id == u1.Id;
    public Mock<DbSet<User>> MockUsersSet;

    public MockUsers(MockAppDbContext mockAppDbContext) : base(mockAppDbContext)
    {
        MockUsersSet = MockAppDbContext.SetupMockDbSet<User>(
        [
            new User
            {
                Id = 0,
                AccessFailedCount = 0,
                Chats = [],
                ModeratedChats = [],
                OwnedChats = [],
                UserName = "TestUser0",
                Email = "test_user0@gmail.com",
                EmailConfirmed = true
            },
            new User
            {
                Id = 1,
                AccessFailedCount = 0,
                Chats = [],
                ModeratedChats = [],
                OwnedChats = [],
                UserName = "TestUser1",
                Email = "test_user1@gmail.com",
                EmailConfirmed = true
            },
            new User
            {
                Id = 2,
                AccessFailedCount = 0,
                Chats = [],
                ModeratedChats = [],
                OwnedChats = [],
                UserName = "TestUser2",
                Email = "test_user2@gmail.com",
                EmailConfirmed = true
            },
            new User
            {
                Id = 3,
                AccessFailedCount = 0,
                Chats = [],
                ModeratedChats = [],
                OwnedChats = [],
                UserName = "TestUser3",
                Email = "test_user3@gmail.com",
                EmailConfirmed = true
            }
        ]);
        MockAppDbContext.MockDbContext.Setup(x => x.Users).Returns(MockUsersSet.Object);
    }

    public override void AddMock(User user)
    {
        MockUsersSet = base.AddMock(
            user,
            MockUsersSet,
            _matchFunction
        );
        MockAppDbContext.MockDbContext.Setup(x => x.Users).Returns(MockUsersSet.Object);
    }

    public override void RemoveMock(User user)
    {
        MockUsersSet = base.RemoveMock(
            user,
            MockUsersSet,
            _matchFunction
        );
        MockAppDbContext.MockDbContext.Setup(x => x.Users).Returns(MockUsersSet.Object);
    }

    public override void UpdateMock(User user)
    {
        RemoveMock(user);
        AddMock(user);
    }

    public override User? FindMock(int idx)
    {
        return MockUsersSet.Object.Skip(idx - 1).FirstOrDefault();
    }

    public override void ClearAll()
    {
        var list = MockUsersSet.Object.ToList();
        foreach (var user in list)
        {
            RemoveMock(user);
        }
    }
}