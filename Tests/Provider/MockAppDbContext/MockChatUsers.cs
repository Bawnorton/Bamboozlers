using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MockQueryable.Moq;

namespace Tests.Provider.MockAppDbContext;

public class MockChatUsers : AbstractMockDbSet<ChatUser>
{
    protected override Func<ChatUser, ChatUser, bool> MatchPredicate { get; set; } =
        (c0, c1) => c0.ChatId == c1.ChatId && c0.UserId == c1.UserId;
    
    public MockChatUsers(MockAppDbContext mockAppDbContext, DbSet<Chat> chats) : base(mockAppDbContext)
    {
        var list = new List<ChatUser>();
        foreach (var chat in chats)
        {
            list.AddRange(
                chat.Users.Select(mod => new ChatUser(mod.Id, chat.ID)
                    {
                        Chat = chat, 
                        User = mod 
                    }
                )
            );
        }
        MockDbSet = MockAppDbContext.SetupMockDbSet(list);
    }

    public override void RebindMocks()
    {
        MockAppDbContext.MockDbContext.Setup(x => x.ChatUsers).Returns(GetMocks());
    }
}