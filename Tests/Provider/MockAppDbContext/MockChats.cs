using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;

namespace Tests.Provider.MockAppDbContext;

public class MockChats : AbstractMockDbSet<Chat>
{
    private readonly Func<Chat, Chat, bool> _matchFunction = (c0, c1) => c0.ID == c1.ID;
    public Mock<DbSet<Chat>> MockChatSet;

    public MockChats(MockAppDbContext mockAppDbContext, IQueryable<User> users) : base(mockAppDbContext)
    {
        var user1 = users.First();
        var user2 = users.Skip(1).First();
        var user3 = users.Skip(2).First();

        List<Chat> chats =
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
        ];

        foreach (var chat in chats)
        {
            foreach (var user in chat.Users)
            {
                var userChat = user.Chats.FirstOrDefault(c => _matchFunction(c, chat));
                if (userChat is not null) continue;
                user.Chats.Add(chat);

                if (chat is GroupChat groupChat)
                {
                    var isMod = groupChat.Moderators.FirstOrDefault(m => m.Id == user.Id) is not null;
                    var isOwner = user.Id == groupChat.OwnerID;

                    var ownerChat = user.OwnedChats.FirstOrDefault(gc => _matchFunction(gc, groupChat));
                    var moddedChat = user.ModeratedChats.FirstOrDefault(gc => _matchFunction(gc, groupChat));

                    if (ownerChat is not null && moddedChat is not null) continue;

                    if (ownerChat is null && isOwner)
                        user.OwnedChats.Add(groupChat);

                    if (moddedChat is null && isMod)
                        user.ModeratedChats.Add(groupChat);
                }

                MockAppDbContext.MockUsers.UpdateMock(user);
            }
        }

        MockChatSet = MockAppDbContext.SetupMockDbSet(chats);
        UpdateMockSetup();
    }

    private Mock<DbSet<GroupChat>> FilterGroups()
    {
        return MockChatSet.Object.OfType<GroupChat>().AsQueryable().BuildMockDbSet();
    }

    private void UpdateMockSetup()
    {
        foreach (var groupChat in MockChatSet.Object.OfType<GroupChat>())
        {
            foreach (var user in groupChat.Users)
            {
                var isMod = groupChat.Moderators.FirstOrDefault(m => m.Id == user.Id) is not null;
                var isOwner = user.Id == groupChat.OwnerID;

                var ownerChat = user.OwnedChats.FirstOrDefault(gc => _matchFunction(gc, groupChat));
                var moddedChat = user.ModeratedChats.FirstOrDefault(gc => _matchFunction(gc, groupChat));

                if (ownerChat is not null && moddedChat is not null) continue;

                if (ownerChat is null && isOwner)
                {
                    user.OwnedChats.Add(groupChat);
                }
                else if (ownerChat is not null && !isOwner)
                {
                    user.OwnedChats.Remove(ownerChat);
                }

                if (moddedChat is null && isMod)
                {
                    user.ModeratedChats.Add(groupChat);
                }
                else if (moddedChat is not null && !isMod)
                {
                    user.OwnedChats.Remove(moddedChat);
                }

                MockAppDbContext.MockUsers.UpdateMock(user);
            }
        }

        MockAppDbContext.MockDbContext.Setup(x => x.Chats).Returns(MockChatSet.Object);
        MockAppDbContext.MockDbContext.Setup(x => x.GroupChats).Returns(FilterGroups().Object);
    }

    public override void AddMock(Chat chat)
    {
        MockChatSet = base.AddMock(
            chat,
            MockChatSet,
            _matchFunction
        );

        UpdateMockSetup();
    }

    public override void RemoveMock(Chat chat)
    {
        MockChatSet = base.RemoveMock(
            chat,
            MockChatSet,
            _matchFunction
        );

        UpdateMockSetup();
    }

    public override void UpdateMock(Chat chat)
    {
        RemoveMock(chat);
        AddMock(chat);
    }

    public override Chat? FindMock(int idx)
    {
        return MockChatSet.Object.Skip(idx - 1).FirstOrDefault();
    }

    public override void ClearAll()
    {
        var list = MockChatSet.Object.ToList();
        foreach (var chat in list)
        {
            RemoveMock(chat);
        }
    }
}