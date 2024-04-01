using AngleSharp.Dom;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Data;
using Bamboozlers.Components;
using Bamboozlers.Components.VisualUtility;
using Microsoft.AspNetCore.Http;
using Tests.Provider;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace Tests;

public class UserProfileTests : AuthenticatedBlazoriseTestBase
{
    private new MockDatabaseProvider MockDatabaseProvider;

    private ITestOutputHelper output;
    public UserProfileTests(ITestOutputHelper outputHelper)
    {
        output = outputHelper;
        MockDatabaseProvider = new MockDatabaseProvider(Ctx);
    }

    public async Task<(List<User>,List<Friendship>,List<FriendRequest>,List<Block>)> BuildMockData()
    {
        MockDatabaseProvider.GetMockAppDbContext().MockUsers.ClearAll();
        MockDatabaseProvider.GetMockAppDbContext().MockFriendRequests.ClearAll();
        MockDatabaseProvider.GetMockAppDbContext().MockFriendships.ClearAll();
        MockDatabaseProvider.GetMockAppDbContext().MockBlocks.ClearAll();
        
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
        foreach (var user in users)
        {
            MockDatabaseProvider.GetMockAppDbContext().MockUsers.AddMock(user);
        }

        var friendship = new Friendship
        {
            User1 = users[0],
            User2 = users[1],
            User1ID = users[0].Id,
            User2ID = users[1].Id
        };
        MockDatabaseProvider.GetMockAppDbContext().MockFriendships.AddMock(friendship);

        var request0 = new FriendRequest
        {
            Receiver = users[3],
            ReceiverID = users[3].Id,
            Sender = users[0],
            SenderID = users[0].Id
        };
        var request1 = new FriendRequest
        {
            Receiver = users[0],
            ReceiverID = users[0].Id,
            Sender = users[2],
            SenderID = users[2].Id
        };
        
        MockDatabaseProvider.GetMockAppDbContext().MockFriendRequests.AddMock(request0);
        MockDatabaseProvider.GetMockAppDbContext().MockFriendRequests.AddMock(request1);

        var block0 = new Block
        {
            Blocked = users[0],
            BlockedID = users[0].Id,
            Blocker = users[4],
            BlockerID = users[4].Id
        };
        var block1 = new Block
        {
            Blocked = users[5],
            BlockedID = users[5].Id,
            Blocker = users[0],
            BlockerID = users[0].Id
        };
        MockDatabaseProvider.GetMockAppDbContext().MockBlocks.AddMock(block0);
        MockDatabaseProvider.GetMockAppDbContext().MockBlocks.AddMock(block1);
        
        await SetUser(MockUserManager.CreateMockUser(0));
        UserService.Invalidate();

        return (users, [friendship], [request0, request1], [block0, block1]);
    }
    
    [Fact]
    public async void UserProfileTests_ProfilePopup()
    {
        var (users, friendships, friendRequests, blocks) = await BuildMockData();
        

        for (var i = 0; i <= users.Count; i++)
        {
            output.WriteLine("");
            var focusUser = users[i];
            var component = Ctx.RenderComponent<CompProfileView>(
                parameters 
                    => parameters.Add(p => p.FocusUser, UserRecord.From(focusUser))
            );
            
            output.WriteLine($"user {i}");
            output.WriteLine(component.Markup);
            // Friends and Blocked (By) Users have no action button
            if (i != 1 && i != 4 && i != 5)
            {
                var actionButton = component.Find("#profile-action-button");
                switch (i)
                {
                    case 0: 
                        Assert.True(actionButton.TextContent.Contains("Settings"));
                        break;
                    case 2: 
                        Assert.True(actionButton.TextContent.Contains("Pending"));
                        break;
                    case 3: 
                        Assert.True(actionButton.TextContent.Contains("Accept Friend Request"));
                        break;
                    case 6:
                        Assert.True(actionButton.TextContent.Contains("Send Friend Request"));
                        break;
                }
            }
            
            // Only Self, Friends and Blocked users have badges
            if (i is 0 or 1 or 4)
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
                    case 4: 
                        Assert.True(badge.TextContent.Contains("BLOCKED"));
                        break;
                }
            }
            
            // Every user but Self has an options dropdown
            if (i != 0)
            {
                var badge = component.Find("#profile-actions-dropdown");
            }
        }
    }
}