using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Tests.Provider.MockAppDbContext;

public class MockChatModerators : AbstractMockDbSet<ChatModerator>
{
    protected override Func<ChatModerator, ChatModerator, bool> MatchPredicate { get; set; } =
        (c0, c1) => c0.GroupChatId == c1.GroupChatId && c0.UserId == c1.UserId;
    
    public MockChatModerators(MockAppDbContext mockAppDbContext, DbSet<GroupChat> groupChats) : base(mockAppDbContext)
    {
        var list = new List<ChatModerator>();
        foreach (var groupChat in groupChats)
        {
            list.AddRange(
                groupChat.Moderators.Select(mod => new ChatModerator(mod.Id, groupChat.ID)
                    {
                        GroupChat = groupChat, 
                        User = mod 
                    }
                )
            );
        }
        MockDbSet = MockAppDbContext.SetupMockDbSet(list);
    }

    public override void RebindMocks()
    {
        MockAppDbContext.MockDbContext.Setup(x => x.ChatModerators).Returns(GetMocks());
    }
}