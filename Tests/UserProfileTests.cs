using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Services.UserServices;
using Bamboozlers.Components;
using Microsoft.Extensions.DependencyInjection;
using Tests.Provider;
using Assert = NUnit.Framework.Assert;

namespace Tests;

public class UserProfileTests : AuthenticatedBlazoriseTestBase
{
    private readonly MockDatabaseProvider _mockDatabaseProvider;

    public UserProfileTests()
    {
        _mockDatabaseProvider = new MockDatabaseProvider(Ctx);
        UserInteractionService = new UserInteractionService(AuthService, _mockDatabaseProvider.GetDbContextFactory());
        Ctx.Services.AddSingleton<IUserInteractionService>(UserInteractionService);
    }

    public async Task<(List<User>, List<Friendship>, List<FriendRequest>, List<Block>)> BuildMockData()
    {
        _mockDatabaseProvider.GetMockAppDbContext().MockUsers.ClearAll();
        _mockDatabaseProvider.GetMockAppDbContext().MockFriendRequests.ClearAll();
        _mockDatabaseProvider.GetMockAppDbContext().MockFriendships.ClearAll();
        _mockDatabaseProvider.GetMockAppDbContext().MockBlocks.ClearAll();
        _mockDatabaseProvider.GetMockAppDbContext().MockChats.ClearAll();

        var users = new List<User>
        {
            MockUserManager.CreateMockUser(0),
            MockUserManager.CreateMockUser(1),
            MockUserManager.CreateMockUser(2),
            MockUserManager.CreateMockUser(3),
            MockUserManager.CreateMockUser(4),
            MockUserManager.CreateMockUser(5),
            MockUserManager.CreateMockUser(6)
        };
        foreach (var user in users) _mockDatabaseProvider.GetMockAppDbContext().MockUsers.AddMock(user);

        var friendship = new Friendship(users[0].Id, users[1].Id)
        {
            User1 = users[0],
            User2 = users[1]
        };
        _mockDatabaseProvider.GetMockAppDbContext().MockFriendships.AddMock(friendship);

        var request0 = new FriendRequest(users[0].Id, users[3].Id)
        {
            Receiver = users[3],
            Sender = users[0]
        };
        var request1 = new FriendRequest(users[2].Id, users[0].Id)
        {
            Receiver = users[0],
            Sender = users[2]
        };

        _mockDatabaseProvider.GetMockAppDbContext().MockFriendRequests.AddMock(request0);
        _mockDatabaseProvider.GetMockAppDbContext().MockFriendRequests.AddMock(request1);

        var block0 = new Block(users[0].Id, users[4].Id)
        {
            Blocked = users[0],
            Blocker = users[4]
        };
        var block1 = new Block(users[5].Id, users[0].Id)
        {
            Blocked = users[5],
            Blocker = users[0]
        };
        _mockDatabaseProvider.GetMockAppDbContext().MockBlocks.AddMock(block0);
        _mockDatabaseProvider.GetMockAppDbContext().MockBlocks.AddMock(block1);

        var group1 = new GroupChat
        {
            ID = 1,
            Name = "TestGroup1",
            Owner = users[0],
            OwnerID = 0,
            Users = [users[0]],
            Moderators = []
        };
        _mockDatabaseProvider.GetMockAppDbContext().MockChats.AddMock(group1);

        await SetUser(users[0]);
        UserService.Invalidate();

        return (users, [friendship], [request0, request1], [block0, block1]);
    }

    [Fact]
    public async void UserProfileTests_ProfilePopup()
    {
        var (users, _, _, _) = await BuildMockData();

        for (var i = 0; i < users.Count; i++)
        {
            var focusUser = users[i];
            var component = Ctx.RenderComponent<CompProfileView>(
                parameters
                    => parameters.Add(p => p.FocusUser, UserRecord.From(focusUser))
            );
            if (i != 1 && i != 4 && i != 5)
            {
                var actionButton = component.Find("#profile-action-button");
                switch (i)
                {
                    case 0:
                        Assert.True(actionButton.TextContent.Contains("Settings"));
                        break;
                    case 2:
                        Assert.True(actionButton.TextContent.Contains("Accept Friend Request"));
                        break;
                    case 3:
                        Assert.True(actionButton.TextContent.Contains("Pending"));
                        break;
                    case 6:
                        Assert.True(actionButton.TextContent.Contains("Send Friend Request"));
                        break;
                }
            }

            // Only Self, Friends and Blocked users have badges
            if (i is 0 or 1 or 5)
            {
                var badge = component.Find("#profile-badge");
                switch (i)
                {
                    case 0:
                        Assert.True(badge.TextContent.Contains("YOU"));
                        break;
                    case 1:
                        Assert.True(badge.TextContent.Contains("FRIEND"));
                        break;
                    case 5:
                        Assert.True(badge.TextContent.Contains("BLOCKED"));
                        break;
                }
            }

            // Every user but Self has an options dropdown
            if (i != 0)
            {
                component.Find("#profile-actions-dropdown");

                switch (i)
                {
                    case 1:
                        Assert.DoesNotThrow(() => component.Find("#unfriend-option"));
                        Assert.DoesNotThrow(() => component.Find("#block-option"));
                        Assert.DoesNotThrow(() => component.Find("#TestGroup1-invite-option"));
                        break;
                    case 2:
                        Assert.DoesNotThrow(() => component.Find("#decline-request-option"));
                        Assert.DoesNotThrow(() => component.Find("#block-option"));
                        break;
                    case 3:
                        Assert.DoesNotThrow(() => component.Find("#revoke-request-option"));
                        Assert.DoesNotThrow(() => component.Find("#block-option"));
                        break;
                    case 4:
                        Assert.DoesNotThrow(() => component.Find("#block-option"));
                        break;
                    case 5:
                        Assert.DoesNotThrow(() => component.Find("#unblock-option"));
                        break;
                    case 6:
                        Assert.DoesNotThrow(() => component.Find("#block-option"));
                        break;
                }
            }
        }
    }
}