using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Tests.Provider.MockAppDbContext;

public class MockGroupInvites : AbstractMockDbSet<GroupInvite>
{
    private readonly Func<GroupInvite, GroupInvite, bool> _matchFunction = (i0, i1) =>
        i0.SenderID == i1.SenderID && i0.RecipientID == i1.RecipientID && i0.GroupID == i1.GroupID;

    private Mock<DbSet<GroupInvite>> _mockGroupInvites;

    public MockGroupInvites(MockAppDbContext mockAppDbContext) : base(
        mockAppDbContext)
    {
        _mockGroupInvites = mockAppDbContext.SetupMockDbSet(new List<GroupInvite>());
        MockAppDbContext.MockDbContext.Setup(x => x.GroupInvites).Returns(_mockGroupInvites.Object);
    }

    public override void AddMock(GroupInvite groupInvite)
    {
        _mockGroupInvites = base.AddMock(
            groupInvite,
            _mockGroupInvites,
            _matchFunction
        );
        MockAppDbContext.MockDbContext.Setup(x => x.GroupInvites).Returns(_mockGroupInvites.Object);
    }

    public override void RemoveMock(GroupInvite groupInvite)
    {
        _mockGroupInvites = base.RemoveMock(
            groupInvite,
            _mockGroupInvites,
            _matchFunction
        );
        MockAppDbContext.MockDbContext.Setup(x => x.GroupInvites).Returns(_mockGroupInvites.Object);
    }

    public override void UpdateMock(GroupInvite groupInvite)
    {
        _mockGroupInvites = base.RemoveMock(groupInvite, _mockGroupInvites, _matchFunction);
        _mockGroupInvites = base.AddMock(groupInvite, _mockGroupInvites, _matchFunction);
        MockAppDbContext.MockDbContext.Setup(x => x.GroupInvites).Returns(_mockGroupInvites.Object);
    }

    public override GroupInvite? FindMock(int idx)
    {
        return _mockGroupInvites.Object.Skip(idx - 1).FirstOrDefault();
    }

    public override void ClearAll()
    {
        var list = _mockGroupInvites.Object.ToList();
        foreach (var groupInvite in list)
        {
            RemoveMock(groupInvite);
        }
    }
}