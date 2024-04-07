using System.Linq.Expressions;
using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Tests.Provider.MockAppDbContext;

public class MockMessages : AbstractMockDbSet<Message>
{
    protected override Func<Message, Message, bool> MatchPredicate { get; set; } = (m0, m1) => m0.ID == m1.ID;

    public MockMessages(MockAppDbContext mockAppDbContext, DbSet<User> users, DbSet<Chat> chats) : base(mockAppDbContext)
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

        MockDbSet = MockAppDbContext.SetupMockDbSet(messages);
    }
    
    public override void RebindMocks()
    {
        MockAppDbContext.MockDbContext.Setup(x => x.Messages).Returns(GetMocks());
    }
    
    public override void AddMock(Message message)
    {
        base.AddMock(message);
        
        if (message.Chat?.Messages is null) return;
        var chat = message.Chat;
        var match = chat.Messages.FirstOrDefault(m => MatchPredicate(m, message));
        if (match is not null) return;
        chat.Messages.Add(message);
        MockAppDbContext.MockChats.UpdateMock(chat);
    }
    
    public override void RemoveMock(Message message)
    {
        base.RemoveMock(message);
        
        if (message.Chat?.Messages is null) return;
        var chat = message.Chat;
        var match = chat.Messages.FirstOrDefault(m => MatchPredicate(m, message));
        if (match is null) return;
        chat.Messages.Remove(message);
        MockAppDbContext.MockChats.UpdateMock(chat);
    }

    public override void UpdateMock(Message message)
    {
        RemoveMock(message);
        AddMock(message);
    }
}