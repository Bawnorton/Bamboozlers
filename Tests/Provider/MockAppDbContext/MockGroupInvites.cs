using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Tests.Provider.MockAppDbContext;

public class MockGroupInvites : AbstractMockDbSet<GroupInvite>
{
    public Mock<DbSet<GroupInvite>> mockGroupInvites;
    private readonly Func<GroupInvite, GroupInvite, bool> matchFunction = (i0, i1) =>
        i0.SenderID == i1.SenderID && i0.RecipientID == i1.RecipientID && i0.GroupID == i1.GroupID;

    public MockGroupInvites(MockAppDbContext mockAppDbContext, DbSet<User> users, DbSet<Chat> chats) : base(mockAppDbContext)
    {
        mockGroupInvites = mockAppDbContext.SetupMockDbSet(new List<GroupInvite>());
        MockAppDbContext.MockDbContext.Setup(x => x.GroupInvites).Returns(mockGroupInvites.Object);
    }
    
    public override void AddMock(GroupInvite groupInvite)
    {
        mockGroupInvites = base.AddMock(
            groupInvite,
            mockGroupInvites,
            matchFunction
        );
        MockAppDbContext.MockDbContext.Setup(x => x.GroupInvites).Returns(mockGroupInvites.Object);
    }
    
    public override void RemoveMock(GroupInvite groupInvite)
    {
        mockGroupInvites = base.RemoveMock(
            groupInvite,
            mockGroupInvites,
            matchFunction
        );
        MockAppDbContext.MockDbContext.Setup(x => x.GroupInvites).Returns(mockGroupInvites.Object);
    }
    
    public override void UpdateMock(GroupInvite groupInvite)
    {
        mockGroupInvites = base.RemoveMock(groupInvite, mockGroupInvites, matchFunction);
        mockGroupInvites = base.AddMock(groupInvite, mockGroupInvites, matchFunction);
        MockAppDbContext.MockDbContext.Setup(x => x.GroupInvites).Returns(mockGroupInvites.Object);
    }
    
    public override GroupInvite? FindMock(int idx)
    {
        return mockGroupInvites.Object.Skip(idx - 1).FirstOrDefault();
    }
    
    public override void ClearAll()
    {
        var list = mockGroupInvites.Object.ToList();
        foreach (var groupInvite in list)
        {
            RemoveMock(groupInvite);
        }
    }
}