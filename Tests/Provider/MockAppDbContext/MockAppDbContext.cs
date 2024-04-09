using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;

namespace Tests.Provider.MockAppDbContext;

public class MockAppDbContext
{
    public Mock<AppDbContext> MockDbContext { get; set; }
    public MockUsers MockUsers { get; set; }
    public MockChats MockChats { get; set; }
    public MockChatUsers MockChatUsers { get; set; }
    public MockChatModerators MockChatModerators { get; set; }
    public MockMessages MockMessages { get; set; }
    public MockBlocks MockBlocks { get; set; }
    public MockFriendRequests MockFriendRequests { get; set; }
    public MockFriendships MockFriendships{ get; set; }
    public MockGroupInvites MockGroupInvites { get; set; }

    public MockAppDbContext(Mock<AppDbContext> mockDbContext)
    {
        MockDbContext = mockDbContext;

        MockUsers = new MockUsers(this);
        MockChats = new MockChats(this, MockUsers.MockDbSet.Object);
        MockMessages = new MockMessages(this, MockUsers.MockDbSet.Object, MockChats.MockDbSet.Object);
        MockBlocks = new MockBlocks(this, MockUsers.MockDbSet.Object);
        MockFriendRequests = new MockFriendRequests(this, MockUsers.MockDbSet.Object);
        MockFriendships = new MockFriendships(this, MockUsers.MockDbSet.Object);
        MockGroupInvites = new MockGroupInvites(this, MockUsers.MockDbSet.Object, MockChats.MockDbSet.Object);
        MockChatUsers = new MockChatUsers(this, MockChats.MockDbSet.Object);
        MockChatModerators = new MockChatModerators(this, MockChats.GetGroups());
        
        MockChats.SetupChatRelationships(MockChats.MockDbSet.Object.ToList());
        
        BindMocks();
    }

    // ReSharper disable always EntityFramework.UnsupportedServerSideFunctionCall
    private void BindMocks()
    {
        MockDbContext.Setup(x => x.BlockList).Returns(MockBlocks.GetMocks());
        MockDbContext.Setup(x => x.BlockList.Add(It.IsAny<Block>()))
            .Returns((Block block) =>
            {
                MockBlocks.AddMock(block);
                return new MockEntityEntry<Block>(block).GetEntry();
            });
        MockDbContext.Setup(x => x.BlockList.Add(It.IsAny<Block>()))
            .Returns((Block block) =>
            {
                MockBlocks.RemoveMock(block);
                return new MockEntityEntry<Block>(block).GetEntry();
            });
        
        MockDbContext.Setup(x => x.Chats).Returns(MockChats.GetMocks());
        MockDbContext.Setup(x => x.Chats.Add(It.IsAny<Chat>()))
            .Returns((Chat chat) =>
            {
                MockChats.AddMock(chat);
                return new MockEntityEntry<Chat>(chat).GetEntry();
            });
        
        MockDbContext.Setup(x => x.GroupChats).Returns(MockChats.GetGroups());
        MockDbContext.Setup(x => x.GroupChats.Add(It.IsAny<GroupChat>()))
            .Returns((GroupChat groupChat) =>
            {
                MockChats.AddMock(groupChat);
                return new MockEntityEntry<GroupChat>(groupChat).GetEntry();
            });
        MockDbContext.Setup(x => x.GroupChats.Remove(It.IsAny<GroupChat>()))
            .Returns((GroupChat groupChat) =>
            {
                MockChats.RemoveMock(groupChat);
                return new MockEntityEntry<GroupChat>(groupChat).GetEntry();
            });
        
        MockDbContext.Setup(x => x.FriendRequests).Returns(MockFriendRequests.GetMocks());
        MockDbContext.Setup(x => x.FriendRequests.Add(It.IsAny<FriendRequest>()))
            .Returns((FriendRequest friendRequest) =>
            {
                MockFriendRequests.AddMock(friendRequest);
                return new MockEntityEntry<FriendRequest>(friendRequest).GetEntry();
            });
        MockDbContext.Setup(x => x.FriendRequests.Remove(It.IsAny<FriendRequest>()))
            .Returns((FriendRequest friendRequest) =>
            {
                MockFriendRequests.RemoveMock(friendRequest);
                return new MockEntityEntry<FriendRequest>(friendRequest).GetEntry();
            });
        
        MockDbContext.Setup(x => x.FriendShips).Returns(MockFriendships.GetMocks());
        MockDbContext.Setup(x => x.FriendShips.Add(It.IsAny<Friendship>()))
            .Returns((Friendship friendship) =>
            {
                MockFriendships.AddMock(friendship);
                return new MockEntityEntry<Friendship>(friendship).GetEntry();
            });
        MockDbContext.Setup(x => x.FriendShips.Remove(It.IsAny<Friendship>()))
            .Returns((Friendship friendship) =>
            {
                MockFriendships.RemoveMock(friendship);
                return new MockEntityEntry<Friendship>(friendship).GetEntry();
            });
        
        MockDbContext.Setup(x => x.GroupInvites).Returns(MockGroupInvites.GetMocks());
        MockDbContext.Setup(x => x.GroupInvites.Add(It.IsAny<GroupInvite>()))
            .Returns((GroupInvite groupInvite) =>
            {
                MockGroupInvites.AddMock(groupInvite);
                return new MockEntityEntry<GroupInvite>(groupInvite).GetEntry();
            });
        MockDbContext.Setup(x => x.GroupInvites.Remove(It.IsAny<GroupInvite>()))
            .Returns((GroupInvite groupInvite) =>
            {
                MockGroupInvites.RemoveMock(groupInvite);
                return new MockEntityEntry<GroupInvite>(groupInvite).GetEntry();
            });
        
        MockDbContext.Setup(x => x.Messages).Returns(MockMessages.GetMocks());
        MockDbContext.Setup(x => x.Messages.Add(It.IsAny<Message>()))
            .Returns((Message message) =>
            {
                MockMessages.AddMock(message);
                return new MockEntityEntry<Message>(message).GetEntry();
            });
        MockDbContext.Setup(x => x.Messages.Remove(It.IsAny<Message>()))
            .Returns((Message message) =>
            {
                MockMessages.RemoveMock(message);
                return new MockEntityEntry<Message>(message).GetEntry();
            });
        
        MockDbContext.Setup(x => x.Users).Returns(MockUsers.GetMocks());
        MockDbContext.Setup(x => x.Users.Add(It.IsAny<User>()))
            .Returns((User user) =>
            {
                MockUsers.AddMock(user);
                return new MockEntityEntry<User>(user).GetEntry();
            });
        MockDbContext.Setup(x => x.Users.Remove(It.IsAny<User>()))
            .Returns((User user) =>
            {
                MockUsers.RemoveMock(user);
                return new MockEntityEntry<User>(user).GetEntry();
            });
        
        MockDbContext.Setup(x => x.ChatUsers).Returns(MockChatUsers.GetMocks());
        MockDbContext.Setup(x => x.ChatUsers.Add(It.IsAny<ChatUser>()))
            .Returns((ChatUser chatUser) =>
            {
                MockChatUsers.AddMock(chatUser);
                return new MockEntityEntry<ChatUser>(chatUser).GetEntry();
            });
        MockDbContext.Setup(x => x.ChatUsers.Remove(It.IsAny<ChatUser>()))
            .Returns((ChatUser chatUser) =>
            {
                MockChatUsers.RemoveMock(chatUser);
                return new MockEntityEntry<ChatUser>(chatUser).GetEntry();
            });
        
        MockDbContext.Setup(x => x.ChatModerators).Returns(MockChatModerators.GetMocks());
        MockDbContext.Setup(x => x.ChatModerators.Add(It.IsAny<ChatModerator>()))
            .Returns((ChatModerator chatModerator) =>
            {
                MockChatModerators.AddMock(chatModerator);
                return new MockEntityEntry<ChatModerator>(chatModerator).GetEntry();
            });
        MockDbContext.Setup(x => x.ChatModerators.Remove(It.IsAny<ChatModerator>()))
            .Returns((ChatModerator chatModerator) =>
            {
                MockChatModerators.RemoveMock(chatModerator);
                return new MockEntityEntry<ChatModerator>(chatModerator).GetEntry();
            });
    }
    
    private int FindEntryIndex<T>(T entry,
        List<T> entriesList,
        Func<T, T, bool> matchPredicate) where T : class
    {
        var entryMatch = entriesList.FirstOrDefault(e => matchPredicate.Invoke(e,entry));
        return entryMatch is not null 
            ? entriesList.IndexOf(entryMatch)
            : -1;
    }
    
    public Mock<DbSet<T>> AddMockDbEntry<T>(T entry, 
        Mock<DbSet<T>> entries, 
        Func<T,T,bool> matchPredicate) where T : class
    {
        var entriesList = entries.Object.ToList();
        var matchIdx = FindEntryIndex(entry, entriesList, matchPredicate);
        if (matchIdx != -1)
        {
            entriesList[matchIdx] = entry;
        }
        else
        {
            entriesList.Add(entry);
        }
        entries = SetupMockDbSet(entriesList);
        return entries;
    }
    
    public Mock<DbSet<T>> RemoveMockDbEntry<T>(T entry, 
        Mock<DbSet<T>> entries, 
        Func<T,T,bool> matchPredicate) where T : class
    {
        var entriesList = entries.Object.ToList();
        var matchIdx = FindEntryIndex(entry, entriesList, matchPredicate);

        if (matchIdx != -1)
        {
            entriesList.RemoveAt(matchIdx);
        }
        entries = SetupMockDbSet(entriesList);
        return entries;
    }
    
    public Mock<DbSet<T>> SetupMockDbSet<T>(IEnumerable<T> data) where T : class
    {
        var set = data.AsQueryable().BuildMockDbSet();
        set.Setup(x => x.Add(It.IsAny<T>()))
            .Returns((T entity) =>
            {
                var mEe =  new MockEntityEntry<T>(entity);
                return mEe.GetEntry();
            });
        
        return set;
    }
}