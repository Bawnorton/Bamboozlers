using System.Linq.Expressions;
using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Tests.Provider.MockAppDbContext;

public class MockChats : AbstractMockDbSet<Chat>
{
    public Mock<DbSet<Chat>> mockChats;
    private readonly Func<Chat, Chat, bool> matchFunction = (c0, c1) => c0.ID == c1.ID;
    
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
        ]);

        foreach (var chat in chats)
        {
            foreach (var user in chat.Users)
            {
                var userChat = user.Chats.FirstOrDefault(c => matchFunction(c, chat));
                if (userChat is not null) continue;
                user.Chats.Add(chat);
                MockAppDbContext.MockUsers.UpdateMock(user);
            }
        }
        
        mockChats = MockAppDbContext.SetupMockDbSet(chats);
        MockAppDbContext.MockDbContext.Setup(x => x.Chats).Returns(mockChats.Object);
    }
    
    public override void AddMock(Chat chat)
    {
        mockChats = base.AddMock(
            chat,
            mockChats,
            matchFunction
        );
        
        MockAppDbContext.MockDbContext.Setup(x => x.Chats).Returns(mockChats.Object);
        
        foreach (var user in chat.Users)
        {
            var userChat = user.Chats.FirstOrDefault(c => matchFunction(c, chat));
            if (userChat is not null) continue;
            user.Chats.Add(chat);
            MockAppDbContext.MockUsers.UpdateMock(user);
        }
    }
    
    public override void RemoveMock(Chat chat)
    {
        mockChats = base.RemoveMock(
            chat,
            mockChats,
            matchFunction
        );
        
        MockAppDbContext.MockDbContext.Setup(x => x.Chats).Returns(mockChats.Object);
        
        foreach (var user in chat.Users)
        {
            var userChat = user.Chats.FirstOrDefault(c => matchFunction(c, chat));
            if (userChat is null) continue;
            user.Chats.Remove(userChat);
            MockAppDbContext.MockUsers.UpdateMock(user);
        }
    }
    
    public override void UpdateMock(Chat chat)
    {
        RemoveMock(chat);
        AddMock(chat);
    }
    
    public override Chat? FindMock(int idx)
    {
        return mockChats.Object.Skip(idx - 1).FirstOrDefault();
    }
    
    public override void ClearAll()
    {
        var list = mockChats.Object.ToList();
        foreach (var chat in list)
        {
            RemoveMock(chat);
        }
    }
}