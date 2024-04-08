using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Services.UserServices;
using Bamboozlers.Components.MainVisual;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Tests.Provider;
using Xunit.Abstractions;

namespace Tests.CoreTests;

public class UserProfileTests : AuthenticatedBlazoriseTestBase
{
    private new MockDatabaseProvider MockDatabaseProvider { get; set; }
    public UserProfileTests()
    {
        MockDatabaseProvider = new MockDatabaseProvider(Ctx);
        UserInteractionService = new UserInteractionService(AuthService, MockDatabaseProvider.GetDbContextFactory());
        Ctx.Services.AddSingleton<IUserInteractionService>(UserInteractionService);
    }

    private async Task<(List<User>, List<Friendship>, List<FriendRequest>, List<Block>)> BuildMockData()
    {
        MockDatabaseProvider.GetMockAppDbContext().MockUsers.ClearAll();
        MockDatabaseProvider.GetMockAppDbContext().MockFriendRequests.ClearAll();
        MockDatabaseProvider.GetMockAppDbContext().MockFriendships.ClearAll();
        MockDatabaseProvider.GetMockAppDbContext().MockBlocks.ClearAll();
        MockDatabaseProvider.GetMockAppDbContext().MockChats.ClearAll();

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
        foreach (var user in users) MockDatabaseProvider.GetMockAppDbContext().MockUsers.AddMock(user);

        var friendship = new Friendship(users[0].Id, users[1].Id)
        {
            User1 = users[0],
            User2 = users[1]
        };
        MockDatabaseProvider.GetMockAppDbContext().MockFriendships.AddMock(friendship);

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

        MockDatabaseProvider.GetMockAppDbContext().MockFriendRequests.AddMock(request0);
        MockDatabaseProvider.GetMockAppDbContext().MockFriendRequests.AddMock(request1);

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
        MockDatabaseProvider.GetMockAppDbContext().MockBlocks.AddMock(block0);
        MockDatabaseProvider.GetMockAppDbContext().MockBlocks.AddMock(block1);

        var group1 = new GroupChat(0)
        {
            ID = 1,
            Name = "TestGroup1",
            Owner = users[0],
            Users = [users[0]],
            Moderators = []
        };
        MockDatabaseProvider.GetMockAppDbContext().MockChats.AddMock(group1);

        await SetUser(users[0]);

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
            if (i != 4 && i != 5)
            {
                var actionButton = component.Find("#profile-action-button");
                switch (i)
                {
                    case 0:
                        Assert.Contains("Settings", actionButton.TextContent);
                        break;
                    case 1:
                        Assert.Contains("Send Message", actionButton.TextContent);
                        break;
                    case 2:
                        Assert.Contains("Accept Friend Request", actionButton.TextContent);
                        break;
                    case 3:
                        Assert.Contains("Pending", actionButton.TextContent);
                        break;
                    case 6:
                        Assert.Contains("Send Friend Request", actionButton.TextContent);
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
                        Assert.Contains("YOU",badge.TextContent);
                        break;
                    case 1:
                        Assert.Contains("FRIEND",badge.TextContent);
                        break;
                    case 5:
                        Assert.Contains("BLOCKED",badge.TextContent);
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
                        NUnit.Framework.Assert.DoesNotThrow(() => component.Find("#unfriend-option"));
                        NUnit.Framework.Assert.DoesNotThrow(() => component.Find("#block-option"));
                        NUnit.Framework.Assert.DoesNotThrow(() => component.Find("#TestGroup1-invite-option"));
                        break;
                    case 2:
                        NUnit.Framework.Assert.DoesNotThrow(() => component.Find("#decline-request-option"));
                        NUnit.Framework.Assert.DoesNotThrow(() => component.Find("#block-option"));
                        break;
                    case 3:
                        NUnit.Framework.Assert.DoesNotThrow(() => component.Find("#revoke-request-option"));
                        NUnit.Framework.Assert.DoesNotThrow(() => component.Find("#block-option"));
                        break;
                    case 4:
                        NUnit.Framework.Assert.DoesNotThrow(() => component.Find("#block-option"));
                        break;
                    case 5:
                        NUnit.Framework.Assert.DoesNotThrow(() => component.Find("#unblock-option"));
                        break;
                    case 6:
                        NUnit.Framework.Assert.DoesNotThrow(() => component.Find("#block-option"));
                        break;
                }
            }
        }
    }

    [Theory]
    [InlineData("#unfriend-option")]
    [InlineData("#block-option")]
    public async void UserProfileTests_WarningOptionTest(string optionElementId)
    {
        var (users, _, _, _) = await BuildMockData();

        AlertPopupArgs? returnedAlertPopupArgs = null;
        var fauxAlertPopupCallback = EventCallback.Factory.Create<AlertPopupArgs>(
            this,
            args =>
            {
                returnedAlertPopupArgs = args;
            } 
        );
        var focusUser = users[1];
        var component = Ctx.RenderComponent<CompProfileView>(parameters =>
        {
            parameters.Add(p => p.FocusUser, UserRecord.From(focusUser));
            parameters.Add(p => p.OpenAlertPopup, fauxAlertPopupCallback);
        });

        var unfriendButton = component.Find(optionElementId);
        await unfriendButton.ClickAsync(new MouseEventArgs());
        Assert.NotNull(returnedAlertPopupArgs);

        await component.InvokeAsync(() => returnedAlertPopupArgs.OnConfirmCallback.InvokeAsync());
    }

    [Fact]
    public async void UserProfileTests_OpenSettingsPopupTest()
    {
        var (users, _, _, _) = await BuildMockData();
        KnownPopupArgs? returnedKnownPopupCallbackArgs = null;
        var fauxOpenKnownPopupCallback = EventCallback.Factory.Create<KnownPopupArgs>(
            this,
            args =>
            {
                returnedKnownPopupCallbackArgs = args;
            } 
        );
        var focusUser = users[0];
        var component = Ctx.RenderComponent<CompProfileView>(parameters =>
        {
            parameters.Add(p => p.FocusUser, UserRecord.From(focusUser));
            parameters.Add(p => p.OpenKnownPopup, fauxOpenKnownPopupCallback);
        });
        
        var actionButton = component.Find("#profile-action-button");
        await actionButton.ClickAsync(new MouseEventArgs());
        Assert.NotNull(returnedKnownPopupCallbackArgs);
        Assert.Equal(PopupType.Settings, returnedKnownPopupCallbackArgs!.Type);
    }
    
    [Fact]
    public async void UserProfileTests_OpenChatTest()
    {
        var (users, _, _, _) = await BuildMockData();

        OpenChatArgs? returnedChatCallbackArgs = null;
        var fauxOpenChatCallback = EventCallback.Factory.Create<OpenChatArgs>(
            this,
            args =>
            {
                returnedChatCallbackArgs = args;
            } 
        );
        var focusUser = users[1];
        var component = Ctx.RenderComponent<CompProfileView>(parameters =>
        {
            parameters.Add(p => p.FocusUser, UserRecord.From(focusUser));
            parameters.Add(p => p.OpenChatCallback, fauxOpenChatCallback);
        });

        var actionButton = component.Find("#profile-action-button");
        await actionButton.ClickAsync(new MouseEventArgs());
        Assert.NotNull(returnedChatCallbackArgs);
        Assert.Equal(returnedChatCallbackArgs!.Id, focusUser.Id);
        Assert.Equal(ChatType.Dm, returnedChatCallbackArgs.ChatType);
    }

    [Fact]
    public async Task UserProfileTests_OpenInvitePopupTest()
    {
        var (users, _, _, _) = await BuildMockData();
        KnownPopupArgs? returnedKnownPopupCallbackArgs = null;
        var fauxOpenKnownPopupCallback = EventCallback.Factory.Create<KnownPopupArgs>(
            this,
            args =>
            {
                returnedKnownPopupCallbackArgs = args;
            } 
        );
        var focusUser = users[1];
        var component = Ctx.RenderComponent<CompProfileView>(parameters =>
        {
            parameters.Add(p => p.FocusUser, UserRecord.From(focusUser));
            parameters.Add(p => p.OpenKnownPopup, fauxOpenKnownPopupCallback);
        });
        
        var actionButton = component.Find("#TestGroup1-invite-option");
        await actionButton.ClickAsync(new MouseEventArgs());
        Assert.NotNull(returnedKnownPopupCallbackArgs);
        Assert.Equal(PopupType.InviteGroupMembers, returnedKnownPopupCallbackArgs!.Type);   
    }
}