using System.Linq.Expressions;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Func;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;

namespace Tests.Provider;

public class MockDatabaseProvider
{
    private readonly Mock<IDbContextFactory<AppDbContext>> _mockDbContextFactory;
    private readonly Mock<AppDbContext> _mockDbContext;
    public MockDatabaseProvider(TestContextBase ctx, ref Mock<DbSet<GroupInvite>> mockGroupInvites)
    {
        _mockGroupInvites = ref mockGroupInvites;
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

    private ref struct MockDbSets
    {
        public ref Mock<DbSet<User>> mockUsers;
        public ref Mock<DbSet<Chat>> mockChats;
        public ref Mock<DbSet<Message>> mockMessages;
        public ref Mock<DbSet<Block>> mockBlocks;
        public ref Mock<DbSet<FriendRequest>> mockFriendRequests;
        public ref Mock<DbSet<Friendship>> mockFriendships;
        public ref Mock<DbSet<GroupInvite>> mockGroupInvites;
    }
    
    private void SetupMockDbContext()
    {
        MockDbSets sets = new()
        {
            mockUsers = SetupMockUsers(),
        };
        sets.mockChats = SetupMockChats(sets.mockUsers.Object);
        sets.mockChats = SetupMockChats(sets.mockUsers.Object);
        sets.mockMessages = SetupMockMessages(_mockUsers.Object, _mockChats.Object);
        sets.mockBlocks = SetupMockBlocks(_mockUsers.Object);
        sets.mockFriendRequests = SetupMockFriendRequests(_mockUsers.Object);
        sets.mockFriendships = SetupMockFriendships(_mockUsers.Object);
        sets.mockGroupInvites = SetupMockDbSet(new List<GroupInvite>());
        
        _mockDbContext.Setup(x => x.GroupInvites).Returns(_mockGroupInvites.Object);
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

        SetupMockUsers(users);
    }

    public bool RemoveMockUser(User user)
    {
        var users = _mockUsers.Object.ToList();
        var res = users.Remove(user);
        SetupMockUsers(users);
        
        return res;
    }

    public User? GetMockUser(int idx)
    {
        return _mockUsers.Object.FirstOrDefault(u => u.Id == idx);
    }
    
    public void ClearMockUsers()
    {
        SetupMockUsers(new List<User>());
    }

    private Mock<DbSet<User>> SetupMockUsers(List<User>? values = null)
    {
        Mock<DbSet<User>> dbSet = SetupMockDbSet(values ?? 
        [
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
            }
        ]);  
        _mockDbContext.Setup(x => x.Users).Returns(dbSet.Object);
        return dbSet;
    }
    
    public void AddMockChat(Chat chat)
    {
        var chats = _mockChats.Object.ToList();
        var match = chats.FirstOrDefault(c => c.ID == chat.ID);
        if (match is not null)
        {
            chats[chats.IndexOf(match)] = chat;
        }
        else
        {
            chats.Add(chat);
        }
        SetupMockChats(_mockUsers.Object, chats);
    }

    private Mock<DbSet<Chat>> SetupMockChats(IQueryable<User> users, List<Chat>? values = null)
    {
        var user1 = users.First();
        var user2 = users.Skip(1).First();
        var user3 = users.Skip(2).First();
        var chats = values ??
        [
            new Chat
            {
                ID = 1,
                Users = new List<User> { user1, user2 },
                Messages = []
            },
            new GroupChat
            {
                ID = 2,
                Name = "TestGroupChat",
                Owner = user1,
                OwnerID = user1.Id,
                Moderators = new List<User> { user2 },
                Users = new List<User> { user1, user2, user3 },
                Messages = []
            }
        ];
        
        foreach (var chat in chats)
        {
            foreach (var user in chat.Users)
            {
                user.Chats.Add(chat);
            }
        }

        var groupChats = new List<GroupChat>();
        foreach (var groupChat in chats.OfType<GroupChat>().ToList())
        {
            groupChat.ID = groupChats.Count;
            groupChats.Add(groupChat);
            
            groupChat.Owner.Chats.Add(groupChat);
            groupChat.Owner.OwnedChats.Add(groupChat);
            
            foreach (var mod in groupChat.Moderators)
            {
                mod.ModeratedChats.Add(groupChat);
            }
        }
        
        var dbSet = SetupMockDbSet(chats);
        var groupDbSet = SetupMockDbSet(groupChats);
        
        _mockDbContext.Setup(x => x.Chats).Returns(dbSet.Object);
        _mockDbContext.Setup(x => x.GroupChats).Returns(groupDbSet.Object);
        
        return dbSet;
    }

    public void AddMockMessage(Message message)
    {
        var messages = _mockMessages.Object.ToList();
        var match = messages.FirstOrDefault(m => m.ID == message.ID);
        if (match is not null)
        {
            messages[messages.IndexOf(match)] = message;
        }
        else
        {
            messages.Add(message);
        }
        SetupMockChats(_mockUsers.Object, message);
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
            var user2 = users.Skip(1).First();
            var user3 = users.Last();

            var dbSet = SetupMockDbSet(new List<Block>
            {
                new()
                {
                    Blocked = user2,
                    BlockedID = user2.Id,
                    Blocker = user3,
                    BlockerID = user3.Id
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

    public void AddMockFriendRequest(FriendRequest friendRequest)
    {
        var dbSet = AddMockDbEntry<FriendRequest>(
            friendRequest, 
            _mockFriendRequests,
            request => request.SenderID == friendRequest.SenderID && request.ReceiverID == friendRequest.ReceiverID
        );
        
        SetupMockFriendships(_mockUsers.Object, friendships.AsQueryable());
    }
    private Mock<DbSet<FriendRequest>> SetupMockFriendRequests(IQueryable<User> users)
    {
        try
        {
            var user1 = users.First();
            var user3 = users.Last();

            var dbSet = SetupMockDbSet(new List<FriendRequest>
            {
                new()
                {
                    Sender = user1,
                    SenderID = user1.Id,
                    Receiver = user3,
                    ReceiverID = user3.Id
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

    public void AddMockFriendship(Friendship friendship)
    {
        var friendships = _mockFriendships.Object.ToList();
        var match = friendships.FirstOrDefault(f => f.User1ID == friendship.User1ID && f.User2ID == friendship.User2ID);
        if (match is not null)
        {
            friendships[friendships.IndexOf(match)] = friendship;
        }
        else
        {
            friendships.Add(friendship);
        }
        SetupMockFriendships(_mockUsers.Object, friendships.AsQueryable());
    }
    
    private Mock<DbSet<Friendship>> SetupMockFriendships(IQueryable<User> users, IQueryable<Friendship>? friendships = null)
    {
        try
        {
            var user0 = users.First();
            var user1 = users.Skip(1).First();
            var user2 = users.Skip(2).First();

            var dbSet = SetupMockDbSet(new List<Friendship>
            {
                new()
                {
                    User1 = user0,
                    User1ID = user0.Id,
                    User2 = user1,
                    User2ID = user1.Id
                },
                new()
                {
                    User1 = user1,
                    User1ID = user1.Id,
                    User2 = user2,
                    User2ID = user2.Id
                },
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

    private void AddMockDbEntry<T>(T entry, ref Mock<DbSet<T>> entries, Func<T, bool> predicate) where T : class
    {
        var entriesList = entries.Object.ToList();
        var entryMatch = entriesList.FirstOrDefault(predicate);
        if (entryMatch is not null)
        {
            entriesList[entriesList.IndexOf(entryMatch)] = entry;
        }
        else
        {
            entriesList.Add(entry);
        }
        entries = SetupMockDbSet(entriesList);
    }
    
    public Mock<DbSet<T>> SetupMockDbSet<T>(IEnumerable<T> data) where T : class
    {
        return data.AsQueryable().BuildMockDbSet();
    }
}