using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Services;
using Bamboozlers.Layout;
using Microsoft.EntityFrameworkCore;

namespace Tests;

public class NavLayoutTests : AuthenticatedBlazoriseTestBase
{
    [Fact]
    public async Task NavLayoutTests_FindAndOpenDms()
    {
        AuthHelper.Invalidate();
        var component = Ctx.RenderComponent<NavLayout>();
        
        await using var db = await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();
        var dms = Self.Chats.Except(Self.Chats.OfType<GroupChat>()).ToList();
        var others = dms.SelectMany(c => c.Users).Where(u => u.Id != Self.Id).ToList();
        
        var expectedCount = others.Count;
        var dmDropdown = component.Find("#dms_dropdown");
        var actual = dmDropdown.ChildElementCount;

        // Assert
        Assert.Equal(expectedCount, actual);
        
        foreach (var user in others)
        {
            component.Find("#user_" + user.Id).Click();
            var text = component.Find("#header-text");
            var expected = (user.DisplayName ?? user.UserName)!;
            
            // Assert
            Assert.Equal(expected, text.TextContent);
        }
    }

    [Fact]
    public async Task NavLayoutTests_FindAndOpenGroups()
    {
        AuthHelper.Invalidate();
        var component = Ctx.RenderComponent<NavLayout>();
        
        await using var db = await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();
        var groups = Self.Chats.OfType<GroupChat>().ToList();
        
        var expectedCount = groups.Count;
        var groupDropdown = component.Find("#groups_dropdown");
        var actual = groupDropdown.ChildElementCount;
        
        // Assert
        Assert.Equal(expectedCount, actual);
        
        foreach (var group in groups)
        {
            component.Find("#group_" + group.ID).Click();
            var text = component.Find("#header-text");
            var expected = group.Name;
            
            // Assert
            Assert.Equal(expected, text.TextContent);
        }
    }

    [Fact]
    public async Task NavLayoutTests_FindAndOpenFriends()
    {
        AuthHelper.Invalidate();
        var component = Ctx.RenderComponent<NavLayout>();
        
        await using var db = await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();
        var friendships = db.FriendShips.Include(f => f.User1).Include(f => f.User2);
        var friends = friendships.Where(f => f.User1ID == Self.Id || f.User2ID == Self.Id).Select(f => f.User1ID == Self.Id ? f.User2 : f.User1).ToList();
        
        var count = friends.Count;
        component.Find("#friends").Click();
        var text = component.Find("#header-text");
        var expected = $"Friends ({count})";
        
        // Assert
        Assert.Equal(expected, text.TextContent);
    }
}