using System.Linq.Expressions;
using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Tests.Provider.MockAppDbContext;

public class MockGroupChats : AbstractMockDbSet<GroupChat>
{
    public Mock<DbSet<GroupChat>> mockGroupChats;
    private readonly Func<Chat, Chat, bool> matchFunction = (gc0, gc1) => gc0.ID == gc1.ID;   
    
    public MockGroupChats(MockAppDbContext mockAppDbContext, IQueryable<GroupChat> groupChats) : base(mockAppDbContext)
    {
        var list = groupChats.ToList();
        foreach (var groupChat in list)
        {
            groupChat.ID = list.Count;
            
            groupChat.Owner.Chats.Add(groupChat);
            groupChat.Owner.OwnedChats.Add(groupChat);
            MockAppDbContext.MockUsers.UpdateMock(groupChat.Owner);
            
            foreach (var mod in groupChat.Moderators)
            {
                mod.ModeratedChats.Add(groupChat);
                MockAppDbContext.MockUsers.UpdateMock(mod);
            }
        }
        mockGroupChats = MockAppDbContext.SetupMockDbSet(groupChats);
        MockAppDbContext.MockDbContext.Setup(x => x.GroupChats).Returns(mockGroupChats.Object);
    }
    
    public override void AddMock(GroupChat groupChat)
    {
        MockAppDbContext.MockChats.AddMock(groupChat);
        
        mockGroupChats = base.AddMock(
            groupChat,
            mockGroupChats,
            matchFunction
        );
        
        MockAppDbContext.MockDbContext.Setup(x => x.GroupChats).Returns(mockGroupChats.Object);
        
        foreach (var user in groupChat.Moderators)
        {
            var ownerChat = user.OwnedChats.FirstOrDefault(gc => matchFunction(gc, groupChat));
            var moddedChat = user.ModeratedChats.FirstOrDefault(gc => matchFunction(gc, groupChat));

            if (ownerChat is not null && moddedChat is not null) continue;
            
            if (ownerChat is null)
                user.OwnedChats.Add(groupChat);
            if (moddedChat is null)
                user.ModeratedChats.Add(groupChat);
            
            MockAppDbContext.MockUsers.UpdateMock(user);
        }
    }
    
    public override void RemoveMock(GroupChat groupChat)
    {
        MockAppDbContext.MockChats.RemoveMock(groupChat);
        mockGroupChats = base.RemoveMock(
            groupChat,
            mockGroupChats,
            matchFunction
        );
        
        MockAppDbContext.MockDbContext.Setup(x => x.GroupChats).Returns(mockGroupChats.Object);
        
        foreach (var user in groupChat.Users)
        {
            var ownerChat = user.OwnedChats.FirstOrDefault(gc => matchFunction(gc, groupChat));
            var moddedChat = user.ModeratedChats.FirstOrDefault(gc => matchFunction(gc, groupChat));

            if (ownerChat is not null)
                user.OwnedChats.Remove(ownerChat);
            if (moddedChat is not null)
                user.ModeratedChats.Remove(moddedChat);
            
            MockAppDbContext.MockUsers.UpdateMock(user);
        }
    }
    
    public override void UpdateMock(GroupChat groupChat)
    {
        RemoveMock(groupChat);
        AddMock(groupChat);
        MockAppDbContext.MockChats.UpdateMock(groupChat);
    }
    
    public override GroupChat? FindMock(int idx)
    {
        return mockGroupChats.Object.Skip(idx - 1).FirstOrDefault();
    }
    
    public override void ClearAll()
    {
        var list = mockGroupChats.Object.ToList();
        foreach (var groupChat in list)
        {
            RemoveMock(groupChat);
        }
    }
}