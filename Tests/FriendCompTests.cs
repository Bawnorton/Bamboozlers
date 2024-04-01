using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Services;
using Bamboozlers.Classes.Services.Authentication;
using Bamboozlers.Components;
using Tests.Provider;
using Microsoft.EntityFrameworkCore;

namespace Tests;

public class FriendCompTests : AuthenticatedBlazoriseTestBase
{
    
    [Fact]
    public async Task FriendCompTests_TabChangesOnClick()
    {
        await SetUser((await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync())
            .Users.First(u => u.Id == 0));
        
        var component = Ctx.RenderComponent<CompFriendsView>();
        
        component.FindAll(".nav-link")[1].Click();
        
        var actual = component.Find("h4").TextContent;
        actual = actual.Substring(0, actual.Length - 1);
        var expected = "Pending Friend Requests - ";
        
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task FriendCompTests_CurrentFriendsTab()
    {
        await SetUser((await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync())
            .Users.First(u => u.Id == 0));
        
        var component = Ctx.RenderComponent<CompFriendsView>();
        var subComponent = Ctx.RenderComponent<CurrentFriends>();
        
        await using var db = await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();
        
        var friendships = db.FriendShips.Include(f => f.User1).Include(f => f.User2);
        var Friends = friendships.Where(f => f.User1ID == Self.Id || f.User2ID == Self.Id).Select(f => f.User1ID == Self.Id ? f.User2 : f.User1).ToList();
        
        foreach (var user in Friends)
        {
            var actual = component.Find("#user_" + user.Id);
            var expected = user.UserName;
            
            // Assert
            Assert.Equal(expected, actual.TextContent);
        }
        
        component.Find("input").Input("w");
        Friends.RemoveAll(user => !user.UserName.Contains("w"));
        
        foreach (var user in Friends)
        {
            var actual = component.Find("#user_" + user.Id);
            var expected = user.UserName;
            
            // Assert
            Assert.Equal(expected, actual.TextContent);
        }
    }
    
    [Fact]
    public async Task FriendCompTests_BlockedUsersTab()
    {
        await SetUser((await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync())
            .Users.First(u => u.Id == 0));

        var component = Ctx.RenderComponent<CompFriendsView>();
        component.FindAll(".nav-link")[2].Click();

        await using var db = await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();

        var blocks = db.BlockList.Include(f => f.Blocked).Include(f => f.Blocker);
        var blockedUsers = blocks.Where(f => f.BlockerID == Self.Id)
            .Select(f => f.Blocked).ToList();
            
        foreach (var user in blockedUsers)
        {
            var actual = component.Find("#user_" + user.Id);
            var expected = user.UserName;

            // Assert
            Assert.Equal(expected, actual.TextContent);
        }
    }

    [Fact]
    public async Task FriendCompTests_AddFriendTab()
    {
        await SetUser((await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync())
            .Users.First(u => u.Id == 0));
        
        var component = Ctx.RenderComponent<CompFriendsView>();
        
        component.FindAll(".nav-link")[3].Click();
        component.Find("input").Input("TestUser3");
        component.Find("button").MouseDown();
        
        var temp = component.Find("h7").TextContent;
        temp = temp.Substring(0, temp.Count() - 2);
        
        //Assert
        Assert.Equal("Friend request sent to TestUser3",temp);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(4)]
    public async Task FriendCompTests_FriendRequestsTab(int userId)
    {
        // await SetUser((await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync())
        //     .Users.First(u => u.Id == 0));
        await SetUser(MockDatabaseProvider.GetMockUser(userId));
        
        var component = Ctx.RenderComponent<PendingFriendRequests>();
        
        await using var db = await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();
        
        var friendReqs = db.FriendRequests.Include(f => f.Sender).Include(f => f.Receiver);

        var outgoing = friendReqs.Where(f => f.SenderID == userId && f.Status == RequestStatus.Pending).Select(f => f.Receiver).ToList();
        var incoming = friendReqs.Where(f => f.ReceiverID == userId && f.Status == RequestStatus.Pending).Select(f => f.Sender).ToList();
        var denied = friendReqs.Where(f => f.SenderID == userId && f.Status == RequestStatus.Denied).Select(f => f.Receiver).ToList();

        foreach (var user in outgoing)
        {
            var actual = component.Find("#outgoingUser_" + user.Id);
            var expected = "Outgoing request to " + user.UserName;

            // Assert
            Assert.Equal(expected, actual.TextContent);
        }
        
        foreach (var user in incoming)
        {
            var actual = component.Find("#incomingUser_" + user.Id);
            var expected = "Incoming request from " + user.UserName;

            // Assert
            Assert.Equal(expected, actual.TextContent);
        }
        
        foreach (var user in denied)
        {
            var actual = component.Find("#deniedUser_" + user.Id);
            var expected = "Friend request to " + user.UserName + " has been denied";

            // Assert
            Assert.Equal(expected, actual.TextContent);
        }
        
        
        var before = component.FindAll("span").Count;
        if (component.FindAll("#Accept").Count > 0)
        {
            component.Find("#Accept").Click();
            var after = component.FindAll("span").Count;
            
            // Assert
            Assert.Equal(1, (before - after));
        }
        component = Ctx.RenderComponent<PendingFriendRequests>();
        
        before = component.FindAll("span").Count;
        if (component.FindAll("#Deny").Count > 0)
        {
            component.Find("#Deny").Click();
            var after = component.FindAll("span").Count;
            
            // Assert
            Assert.Equal(1, (before - after));
        }
        component = Ctx.RenderComponent<PendingFriendRequests>();
        
        before = component.FindAll("span").Count;
        if (component.FindAll("#Cancel").Count > 0)
        {
            component.Find("#Cancel").Click();
            var after = component.FindAll("span").Count;
            
            // Assert
            Assert.Equal(1, (before - after));
        }
        component = Ctx.RenderComponent<PendingFriendRequests>();
        
        before = component.FindAll("span").Count;
        if (component.FindAll("#Resend").Count > 0)
        {
            component.Find("#Resend").Click();
            var after = component.FindAll("span").Count;
            
            // Assert
            Assert.Equal(before, after);
        }
    }
}