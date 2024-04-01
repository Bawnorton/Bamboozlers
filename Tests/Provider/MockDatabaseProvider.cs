using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;

namespace Tests.Provider;

public class MockDatabaseProvider
{
    private readonly Mock<IDbContextFactory<AppDbContext>> _mockDbContextFactory;
    private readonly Mock<AppDbContext> _mockDbContext;
    public MockDatabaseProvider(TestContextBase ctx)
    {
        _mockDbContextFactory = new Mock<IDbContextFactory<AppDbContext>>();

        var options = new DbContextOptions<AppDbContext>();
        _mockDbContext = new Mock<AppDbContext>(options);
        SetupMockDbContext();

        _mockDbContextFactory.Setup(x => x.CreateDbContext()).Returns(_mockDbContext.Object);
        _mockDbContextFactory.Setup(x => x.CreateDbContextAsync(default)).ReturnsAsync(_mockDbContext.Object);

        ctx.Services.AddSingleton(GetDbContextFactory());
    }

    public IDbContextFactory<AppDbContext> GetDbContextFactory()
    {
        return _mockDbContextFactory.Object;
    }

    private Mock<DbSet<User>> _mockUsers;
    
    public void SetupMockDbContext(List<User>? userList = null)
    {
        _mockUsers = SetupMockUsers(userList);
        
        var mockChats = SetupMockChats(_mockUsers.Object);
        var mockMessages = SetupMockMessages(_mockUsers.Object, mockChats.Object);
        var mockBlocks = SetupMockBlocks(_mockUsers.Object);
        var mockFriendRequests = SetupMockFriendRequests(_mockUsers.Object);
        var mockFriendships = SetupMockFriendships(_mockUsers.Object);
        var mockGroupInvites = SetupMockDbSet(new List<GroupInvite>());
        
        _mockDbContext.Setup(x => x.GroupInvites).Returns(mockGroupInvites.Object);
    }

    public void AddMockUser(User user)
    {
        var users = _mockUsers.Object.ToList();
        var match = users.FirstOrDefault(u => u.Id == user.Id);
        if (match is not null)
        {
            users[users.IndexOf(match)] = user;
        }
        else
        {
            users.Add(user);
        }
        SetupMockDbContext(users);
    }

    public bool RemoveMockUser(User user)
    {
        var users = _mockUsers.Object.ToList();
        var res = users.Remove(user);
        SetupMockDbContext(users);
        
        return res;
    }

    public User? GetMockUser(int idx)
    {
        return _mockUsers.Object.FirstOrDefault(u => u.Id == idx);
    }
    
    public void ClearMockUsers()
    {
        SetupMockDbContext(new List<User>());
    }

    private Mock<DbSet<User>> SetupMockUsers(List<User>? values = null)
    {
        var dbSet = SetupMockDbSet(values ?? new List<User>
        {
            new()
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
            new()
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
            new()
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
            new()
            {
                Id = 3,
                AccessFailedCount = 0,
                Chats = [],
                ModeratedChats = [],
                OwnedChats = [],
                UserName = "TestUser3",
                Email = "test_user3@gmail.com",
                EmailConfirmed = true
            },
            new()
            {
            Id = 4,
            AccessFailedCount = 0,
            Chats = [],
            ModeratedChats = [],
            OwnedChats = [],
            UserName = "TestUser4",
            Email = "test_user4@gmail.com",
            EmailConfirmed = true
            }
        });
        _mockDbContext.Setup(x => x.Users).Returns(dbSet.Object);
        return dbSet;
    }

    private Mock<DbSet<Chat>> SetupMockChats(IQueryable<User> users)
    {
        try
        {
            var user1 = users.First();
            var user2 = users.Skip(1).First();
            var user3 = users.Skip(2).First();

            var dm = new Chat
            {
                ID = 1,
                Users = new List<User> { user1, user2 },
                Messages = []
            };
        
            var groupChat = new GroupChat
            {
                ID = 2,
                Name = "TestGroupChat",
                Owner = user1,
                OwnerID = user1.Id,
                Moderators = new List<User> { user2 },
                Users = new List<User> { user1, user2, user3 },
                Messages = []
            };
        
            user1.Chats.Add(dm);
            user1.Chats.Add(groupChat);
            user1.OwnedChats.Add(groupChat);
            user2.Chats.Add(dm);
            user2.Chats.Add(groupChat);
            user2.ModeratedChats.Add(groupChat);
            user3.Chats.Add(groupChat);
        
            var dbSet = SetupMockDbSet(new List<Chat> { dm, groupChat });
            var groupDbSet = SetupMockDbSet(new List<GroupChat> { groupChat });
            _mockDbContext.Setup(x => x.Chats).Returns(dbSet.Object);
            _mockDbContext.Setup(x => x.GroupChats).Returns(groupDbSet.Object);
            return dbSet;
        }
        catch (Exception)
        {
            var dbSet = SetupMockDbSet(new List<Chat>());
            _mockDbContext.Setup(x => x.Chats).Returns(dbSet.Object);
            return dbSet;
        }
    }

    private Mock<DbSet<Message>> SetupMockMessages(IQueryable<User> users, IQueryable<Chat> chats)
    {
        try
        {
            var user1 = users.First();
            var user2 = users.Skip(1).First();
            var dm = chats.First();
            
            var messages = new List<Message>
            {
                new()
                {
                    ID = 1,
                    Chat = dm,
                    ChatID = dm.ID,
                    Sender = user1,
                    SenderID = user1.Id,
                    Content = "Hello World!",
                    SentAt = DateTime.Now.Subtract(TimeSpan.FromMinutes(5))
                },
                new()
                {
                    ID = 2,
                    Chat = dm,
                    ChatID = dm.ID,
                    Sender = user2,
                    SenderID = user2.Id,
                    Content = "Hi!",
                    SentAt = DateTime.Now.Subtract(TimeSpan.FromMinutes(4))
                },
                new()
                {
                    ID = 3,
                    Chat = dm,
                    ChatID = dm.ID,
                    Sender = user2,
                    SenderID = user2.Id,
                    Content = "How are you?",
                    SentAt = DateTime.Now.Subtract(TimeSpan.FromMinutes(3.5))
                },
                new()
                {
                    ID = 4,
                    Chat = dm,
                    ChatID = dm.ID,
                    Sender = user1,
                    SenderID = user1.Id,
                    Content = "I'm good, you?",
                    SentAt = DateTime.Now.Subtract(TimeSpan.FromMinutes(3))
                },
                new()
                {
                    ID = 5,
                    Chat = dm,
                    ChatID = dm.ID,
                    Sender = user2,
                    SenderID = user2.Id,
                    Content = "I'm good too!",
                    SentAt = DateTime.Now.Subtract(TimeSpan.FromMinutes(2))
                }
            };
            
            for (var i = 6; i < 100; i++)
            {
                messages.Add(new Message
                {
                    ID = i,
                    Chat = dm,
                    ChatID = dm.ID,
                    Sender = user1,
                    SenderID = user1.Id,
                    Content = "Test Message " + i,
                    SentAt = DateTime.Now.Subtract(TimeSpan.FromSeconds(101 - i))
                });
            }
            
            dm.Messages = messages;
            
            var dbSet = SetupMockDbSet(messages);
            _mockDbContext.Setup(x => x.Messages).Returns(dbSet.Object);
            return dbSet;
        }
        catch (Exception)
        {
            var dbSet = SetupMockDbSet(new List<Message>());
            _mockDbContext.Setup(x => x.Messages).Returns(dbSet.Object);
            return dbSet;
        }
    }

    private Mock<DbSet<Block>> SetupMockBlocks(IQueryable<User> users)
    {
        try
        {
            var user1 = users.First();
            var user2 = users.Skip(1).First();
            var user3 = users.Skip(3).First();
            var user5 = users.Last();

            var dbSet = SetupMockDbSet(new List<Block>
            {
                new()
                {
                    Blocked = user2,
                    BlockedID = user2.Id,
                    Blocker = user3,
                    BlockerID = user3.Id
                },
                new()
                {
                    Blocked = user5,
                    BlockedID = user5.Id,
                    Blocker = user1,
                    BlockerID = user1.Id
                }
            });
            _mockDbContext.Setup(x => x.BlockList).Returns(dbSet.Object);
            return dbSet;
        }
        catch (Exception)
        {
            var dbSet = SetupMockDbSet(new List<Block>());
            _mockDbContext.Setup(x => x.BlockList).Returns(dbSet.Object);
            return dbSet;
        }
    }

    private Mock<DbSet<FriendRequest>> SetupMockFriendRequests(IQueryable<User> users)
    {
        try
        {
            var user1 = users.First();
            var user3 = users.Skip(3).First();
            var user5 = users.Last();

            var dbSet = SetupMockDbSet(new List<FriendRequest>
            {
                new()
                {
                    Sender = user1,
                    SenderID = user1.Id,
                    Receiver = user3,
                    ReceiverID = user3.Id
                },
                new()
                {
                    Sender = user5,
                    SenderID = user5.Id,
                    Receiver = user3,
                    ReceiverID = user3.Id,
                    Status = RequestStatus.Denied
                }
            });
            _mockDbContext.Setup(x => x.FriendRequests).Returns(dbSet.Object);
            return dbSet;
        }
        catch (Exception)
        {
            var dbSet = SetupMockDbSet(new List<FriendRequest>());
            _mockDbContext.Setup(x => x.FriendRequests).Returns(dbSet.Object);
            return dbSet;
        }
    }

    private Mock<DbSet<Friendship>> SetupMockFriendships(IQueryable<User> users)
    {
        try
        {
            var user1 = users.First();
            var user2 = users.Skip(1).First();
            var user4 = users.Skip(3).First();

            var dbSet = SetupMockDbSet(new List<Friendship>
            {
                new()
                {
                    User1 = user1,
                    User1ID = user1.Id,
                    User2 = user2,
                    User2ID = user2.Id
                },
                new()
                {
                    User1 = user2,
                    User1ID = user2.Id,
                    User2 = user4,
                    User2ID = user4.Id
                }
            });
            _mockDbContext.Setup(x => x.FriendShips).Returns(dbSet.Object);
            return dbSet;
        }     
        catch (Exception)
        {
            var dbSet = SetupMockDbSet(new List<Friendship>());
            _mockDbContext.Setup(x => x.FriendShips).Returns(dbSet.Object);
            return dbSet;
        }
    }

    public Mock<DbSet<T>> SetupMockDbSet<T>(IEnumerable<T> data) where T : class
    {
        return data.AsQueryable().BuildMockDbSet();
    }
}