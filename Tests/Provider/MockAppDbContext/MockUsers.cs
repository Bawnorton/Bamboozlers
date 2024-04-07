using Bamboozlers.Classes.AppDbContext;

namespace Tests.Provider.MockAppDbContext;

public class MockUsers : AbstractMockDbSet<User>
{
    protected override Func<User, User, bool> MatchPredicate { get; set; }  = (u0, u1) => u0.Id == u1.Id;

    public MockUsers(MockAppDbContext mockAppDbContext) : base(mockAppDbContext)
    {
        MockDbSet = MockAppDbContext.SetupMockDbSet<User>(
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
    }
    
    public override void RebindMocks()
    {
        MockAppDbContext.MockDbContext.Setup(x => x.Users).Returns(GetMocks());
    }
}