using AngleSharp.Dom;
using Bamboozlers.Account;
using Bamboozlers.Classes;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Services;
using Bamboozlers.Layout;
using Bamboozlers.Pages;
using Blazorise.Modules;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Tests;

[Collection("Sequential")]
public class NavLayoutTests : BlazoriseTestBase
{
    private readonly MockDatabaseProvider _mockDatabaseProvider;
    private readonly MockAuthenticationProvider _mockAuthenticationProvider;
    
    private readonly User _self;
    
    public NavLayoutTests()
    {
        _mockDatabaseProvider = new MockDatabaseProvider(Ctx);
        _self = _mockDatabaseProvider.GetDbContextFactory().CreateDbContext().Users.First();
        _mockAuthenticationProvider = new MockAuthenticationProvider(Ctx, _self);

        Ctx.Services.AddSingleton(new Mock<IJSModalModule>().Object);
        AuthHelper.Init(_mockAuthenticationProvider.GetAuthStateProvider(), _mockDatabaseProvider.GetDbContextFactory());
    }

    [Fact]
    public async Task NavLayoutTests_FindAndOpenDms()
    {
        AuthHelper.Invalidate();
        var component = Ctx.RenderComponent<NavLayout>(
            parameters => parameters.Add(p => p.Testing, true)
        );
        
        await using var db = await _mockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();
        var dms = _self.Chats.Except(_self.Chats.OfType<GroupChat>()).ToList();
        var others = dms.SelectMany(c => c.Users).Where(u => u.Id != _self.Id).ToList();
        
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
        var component = Ctx.RenderComponent<NavLayout>(
            parameters => parameters.Add(p => p.Testing, true)
        );
        
        await using var db = await _mockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();
        var groups = _self.Chats.OfType<GroupChat>().ToList();
        
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
        var component = Ctx.RenderComponent<NavLayout>(
            parameters => parameters.Add(p => p.Testing, true)
        );
        
        await using var db = await _mockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();
        var friendships = db.FriendShips.Include(f => f.User1).Include(f => f.User2);
        var friends = friendships.Where(f => f.User1ID == _self.Id || f.User2ID == _self.Id).Select(f => f.User1ID == _self.Id ? f.User2 : f.User1).ToList();
        
        var count = friends.Count;
        component.Find("#friends").Click();
        var text = component.Find("#header-text");
        var expected = $"Friends ({count})";
        
        // Assert
        Assert.Equal(expected, text.TextContent);
    }
}