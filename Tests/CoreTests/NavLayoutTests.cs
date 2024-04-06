using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Layout;
using Blazorise;
using Blazorise.Modules;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit.Abstractions;

namespace Tests.CoreTests;

public class NavLayoutTests : AuthenticatedBlazoriseTestBase
{
    public NavLayoutTests()
    {
        Ctx.Services.AddSingleton(new Mock<IJSModalModule>().Object);
        Ctx.Services.AddBlazorise().Replace(ServiceDescriptor.Transient<IComponentActivator, ComponentActivator>());
    }
    
    [Fact]
    public async Task NavLayoutTests_FindAndOpenDms()
    {
        await SetUser((await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync())
            .Users.First(u => u.Id == 0));
        
        var component = Ctx.RenderComponent<NavLayout>();
        
        await using var db = await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();
        var dms = Self!.Chats.Except(Self.Chats.OfType<GroupChat>()).ToList();
        var others = dms.SelectMany(c => c.Users).Where(u => u.Id != Self.Id).ToList();
        
        var expectedCount = others.Count + 2; // 2 non-Chat elements
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
        await SetUser((await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync())
            .Users.First(u => u.Id == 0));
        
        var component = Ctx.RenderComponent<NavLayout>();
        
        await using var db = await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();
        var groups = Self!.Chats.OfType<GroupChat>().ToList();
        
        var expectedCount = groups.Count + 2; // 2 non-Chat elements
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
    public async Task NavLayoutTests_FindAndOpenHome()
    {
        var user = (await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync())
            .Users.First(u => u.Id == 0);
        await SetUser(user);
        
        var component = Ctx.RenderComponent<NavLayout>();
        
        component.Find("#home").Click();
        var text = component.Find("#header-text");
        var expected = $"Hello, {user.UserName}!";
        
        // Assert
        Assert.Equal(expected, text.TextContent);
    }
}