using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Tests.Provider.MockAppDbContext;

public class MockGroupInvites : AbstractMockDbSet<GroupInvite>
{
    protected override Func<GroupInvite, GroupInvite, bool> MatchPredicate { get; set; } = (i0, i1) =>
        i0.SenderID == i1.SenderID && i0.RecipientID == i1.RecipientID && i0.GroupID == i1.GroupID;

    public MockGroupInvites(MockAppDbContext mockAppDbContext, DbSet<User> users, DbSet<Chat> chats) : base(mockAppDbContext)
    {
        MockDbSet = mockAppDbContext.SetupMockDbSet(new List<GroupInvite>());
    }
    
    public override void RebindMocks()
    {
        MockAppDbContext.MockDbContext.Setup(x => x.GroupInvites).Returns(GetMocks());
    }
}