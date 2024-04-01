using System.Linq.Expressions;
using System.Reflection;
using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Tests.Provider.MockAppDbContext;

public class MockUsers : AbstractMockDbSet<User>
{
    public Mock<DbSet<User>> mockUsers;
    private readonly Func<User, User, bool> matchFunction = (u0, u1) => u0.Id == u1.Id;

    public MockUsers(MockAppDbContext mockAppDbContext) : base(mockAppDbContext)
    {
        mockUsers = MockAppDbContext.SetupMockDbSet<User>(
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
        MockAppDbContext.MockDbContext.Setup(x => x.Users).Returns(mockUsers.Object);
    }
    
    public override void AddMock(User user)
    {
        mockUsers = base.AddMock(
            user,
            mockUsers,
            matchFunction
        );
        MockAppDbContext.MockDbContext.Setup(x => x.Users).Returns(mockUsers.Object);
    }
    
    public override void RemoveMock(User user)
    {
        mockUsers = base.RemoveMock(
            user,
            mockUsers,
            matchFunction
        );
        MockAppDbContext.MockDbContext.Setup(x => x.Users).Returns(mockUsers.Object);
    }

    public override void UpdateMock(User user)
    {
        RemoveMock(user);
        AddMock(user);
    }

    public override User? FindMock(int idx)
    {
        return mockUsers.Object.Skip(idx - 1).FirstOrDefault();
    }
    
    public override void ClearAll()
    {
        var list = mockUsers.Object.ToList();
        foreach (var user in list)
        {
            RemoveMock(user);
        }
    }
}