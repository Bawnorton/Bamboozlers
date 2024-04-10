using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MockQueryable.Moq;

namespace Tests.Provider.MockAppDbContext;

public class MockChats : AbstractMockDbSet<Chat>
{
    protected override Func<Chat, Chat, bool> MatchPredicate { get; set; } = (c0, c1) => c0.ID == c1.ID;
    
    public MockChats(MockAppDbContext mockAppDbContext, DbSet<User> users) : base(mockAppDbContext)
    {
        var user1 = users.First();
        var user2 = users.Skip(1).First();
        var user3 = users.Skip(2).First();
        
        var chats = new List<Chat>(
        [
            new Chat
            {
                ID = 1,
                Users = new List<User> { user1, user2 },
                Messages = []
            },
            new GroupChat(user1.Id)
            {
                ID = 2,
                Name = "TestGroupChat",
                Owner = user1,
                Moderators = new List<User> { user2 },
                Users = new List<User> { user1, user2, user3 },
                Messages = []
            }
        ]);
        
        MockDbSet = MockAppDbContext.SetupMockDbSet(chats);
    }

    public override void RebindMocks()
    {
        MockAppDbContext.MockDbContext.Setup(x => x.Chats).Returns(GetMocks());
        MockAppDbContext.MockDbContext.Setup(x => x.GroupChats).Returns(GetGroups());
    }
    
    private Mock<DbSet<GroupChat>> FilterGroups()
    {
        return MockDbSet.Object.OfType<GroupChat>().AsQueryable().BuildMockDbSet();
    }

    public void SetupChatRelationships(List<Chat> chats)
    {
        foreach (var chat in chats)
        {
            foreach (var user in MockAppDbContext.MockUsers.MockDbSet.Object.ToList())
            {
                if (chat is GroupChat groupChat)
                {
                    var ownerChat = user.OwnedChats.FirstOrDefault(c => MatchPredicate(c, groupChat));
                    var isOwner = groupChat.OwnerID == user.Id;
                    if (ownerChat is not null && !isOwner)
                    {
                        user.OwnedChats.Remove(ownerChat);
                    } 
                    else if (ownerChat is null && isOwner)
                    {
                        user.OwnedChats.Add(groupChat);
                    }
                    
                    var modChat = user.ModeratedChats.FirstOrDefault(c => MatchPredicate(c, groupChat));
                    var chatMod = groupChat.Moderators.FirstOrDefault(m => m.Id == user.Id);
                    var mockMod = MockAppDbContext.MockChatModerators.MockDbSet.Object.FirstOrDefault(
                        cm => cm.GroupChatId == groupChat.ID && cm.UserId == user.Id
                    );
                    
                    if (chatMod is null && modChat is not null)
                    {
                        if (mockMod is not null)
                            MockAppDbContext.MockChatModerators.RemoveMock(mockMod);
                        user.ModeratedChats.Remove(modChat);
                    }
                    else if (chatMod is not null && modChat is null)
                    {
                        if (mockMod is null)
                            MockAppDbContext.MockChatModerators.AddMock(new ChatModerator(user.Id,groupChat.ID) {GroupChat = groupChat, User = user});
                        user.ModeratedChats.Add(groupChat);
                    }
                }
                
                var userChat = user.Chats.FirstOrDefault(c => MatchPredicate(c, chat));
                var chatUser = chat.Users.FirstOrDefault(u => u.Id == user.Id);
                var mockChatUser = MockAppDbContext.MockChatUsers.MockDbSet.Object.FirstOrDefault(
                    cu => cu.ChatId == chat.ID && cu.UserId == user.Id
                );
                
                if (chatUser is null && userChat is not null)
                {
                    if (mockChatUser is not null)
                        MockAppDbContext.MockChatUsers.RemoveMock(mockChatUser);
                    user.Chats.Remove(chat);
                }
                else if (chatUser is not null && userChat is null)
                {
                    if (mockChatUser is null)
                        MockAppDbContext.MockChatUsers.AddMock(new ChatUser(user.Id,chat.ID) {Chat = chat, User = user});
                    user.Chats.Add(chat);
                }
                MockAppDbContext.MockUsers.UpdateMock(user);
            }
        }
    }
    
    public override void AddMock(Chat chat)
    {
        if (chat.Users.IsNullOrEmpty())
            chat.Users = [];
        if (chat is GroupChat groupChat)
        {
            if (groupChat.Moderators.IsNullOrEmpty()) 
                groupChat.Moderators = [];
            base.AddMock(groupChat);
        }
        else
        {
            base.AddMock(chat);
        }
        SetupChatRelationships(MockDbSet.Object.ToList());
    }
    
    public override void RemoveMock(Chat chat)
    {
        base.RemoveMock(chat);
        SetupChatRelationships(MockDbSet.Object.ToList());
    }
    
    public override void UpdateMock(Chat chat)
    {
        RemoveMock(chat);
        AddMock(chat);
    }

    public DbSet<GroupChat> GetGroups()
    {
        return FilterGroups().Object;
    }
}