using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Func;
using Bamboozlers.Classes.Utility.Observer;
using Bamboozlers.Layout;
using Blazorise;
using Blazorise.Modules;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

        var component = Ctx.RenderComponent<NavLayout>(parameters => parameters
            .AddCascadingValue<Register<IAsyncKeySubscriber>>(_ => true)
            .AddCascadingValue<Register<IAsyncPacketSubscriber>>(_ => true)
            .Add(p => p.UnregisterPacketSubscribers, _ => 0)
            .Add(p => p.UnregisterKeyPressSubscribers, _ => 0));

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

        var component = Ctx.RenderComponent<NavLayout>(parameters => parameters
            .AddCascadingValue<Register<IAsyncKeySubscriber>>(_ => true)
            .AddCascadingValue<Register<IAsyncPacketSubscriber>>(_ => true)
            .Add(p => p.UnregisterPacketSubscribers, _ => 0)
            .Add(p => p.UnregisterKeyPressSubscribers, _ => 0));

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
            Assert.Contains(expected!, text.TextContent);
        }
    }

    [Fact]
    public async Task NavLayoutTests_FindAndOpenHome()
    {
        var user = (await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync())
            .Users.First(u => u.Id == 0);
        await SetUser(user);
        
        var component = Ctx.RenderComponent<NavLayout>(parameters => parameters
            .AddCascadingValue<Register<IAsyncKeySubscriber>>(_ => true)
            .AddCascadingValue<Register<IAsyncPacketSubscriber>>(_ => true)
            .Add(p => p.UnregisterPacketSubscribers, _ => 0)
            .Add(p => p.UnregisterKeyPressSubscribers, _ => 0));
        
        component.Find("#home").Click();
        var text = component.Find("#header-text");
        var expected = $"Hello, {user.UserName}!";
        
        // Assert
        Assert.Equal(expected, text.TextContent);
    }
    
    [Fact]
    public async Task NavLayoutTests_FindDmFromFriendID()
    {
        await SetUser((await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync())
            .Users.First(u => u.Id == 0));
        
        var component = Ctx.RenderComponent<NavLayout>(parameters => parameters
            .AddCascadingValue<Register<IAsyncKeySubscriber>>(_ => true)
            .AddCascadingValue<Register<IAsyncPacketSubscriber>>(_ => true)
            .Add(p => p.UnregisterPacketSubscribers, _ => 0)
            .Add(p => p.UnregisterKeyPressSubscribers, _ => 0));
        
        await using var db = await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();
        var dms = Self!.Chats.Except(Self.Chats.OfType<GroupChat>()).ToList();
        var others = dms.SelectMany(c => c.Users).Where(u => u.Id != Self.Id).ToList();
        
        foreach (var user in others)
        {
            var id = user.Id;

            await component.Instance.OpenDm(-1, id, true);
            
            var text = component.Find("#header-text");
            var expected = (user.DisplayName ?? user.UserName)!;
            
            // Assert
            Assert.Equal(expected, text.TextContent);
        }
    }
    
    [Fact]
    public async Task NavLayoutTests_FindAndOpenGroupChat()
    {
        await SetUser((await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync())
            .Users.First(u => u.Id == 0));
        
        var component = Ctx.RenderComponent<NavLayout>(parameters => parameters
            .AddCascadingValue<Register<IAsyncKeySubscriber>>(_ => true)
            .AddCascadingValue<Register<IAsyncPacketSubscriber>>(_ => true)
            .Add(p => p.UnregisterPacketSubscribers, _ => 0)
            .Add(p => p.UnregisterKeyPressSubscribers, _ => 0));
        
        await using var db = await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();
        var groups = Self!.Chats.OfType<GroupChat>().ToList();
        
        foreach (var group in groups)
        {
            var id = group.ID;

            await component.Instance.OpenGroup(id, true);
            
            var text = component.Find("#header-text");
            var expected = group.Name;
            
            // Assert
            Assert.Contains(expected!, text.TextContent);
        }
        await component.Instance.OpenGroup(-1);
    }
    
    [Fact]
    public async Task NavLayoutTests_CheckReactionToInvalidChat()
    {
        var user = (await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync())
            .Users.First(u => u.Id == 0);
        await SetUser(user);

        var component = Ctx.RenderComponent<NavLayout>(parameters => parameters
            .AddCascadingValue<Register<IAsyncKeySubscriber>>(_ => true)
            .AddCascadingValue<Register<IAsyncPacketSubscriber>>(_ => true)
            .Add(p => p.UnregisterPacketSubscribers, _ => 0)
            .Add(p => p.UnregisterKeyPressSubscribers, _ => 0));
        
        await using var db = await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();
        var groups = Self!.Chats.OfType<GroupChat>().ToList();
        var dms = Self!.Chats.Except(groups).ToList();

        var subjectDmUser = dms[0].Users.First(u => u.Id != 0);
        component.Find("#user_" + subjectDmUser.Id).Click();
        
        var text = component.Find("#header-text");
        var expected = subjectDmUser.DisplayName ?? subjectDmUser.UserName;
        Assert.Equal(expected, text.TextContent);
        
        user.Chats.Remove(dms[0]);
        MockDatabaseProvider.GetMockAppDbContext().MockUsers.UpdateMock(user);
        await component.Instance.OnUpdate(InteractionEvent.General);
        
        text = component.Find("#header-text");
        expected = "Hello, TestUser0!";
        Assert.Equal(expected, text.TextContent);
        
        var subjectGroup = groups[0];
        component.Find("#group_" + subjectGroup.ID).Click();
        
        text = component.Find("#header-text");
        expected = subjectGroup.Name;
        Assert.Contains(expected!, text.TextContent);
        
        user.Chats.Remove(subjectGroup);
        MockDatabaseProvider.GetMockAppDbContext().MockUsers.UpdateMock(user);
        await component.Instance.OnUpdate(GroupEvent.General);
        
        text = component.Find("#header-text");
        expected = "Hello, TestUser0!";
        Assert.Equal(expected, text.TextContent);
    }
    
    [Fact]
    public async Task NavLayoutTests_CheckReactionToInvalidUser()
    {
        await SetUser((await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync())
            .Users.First(u => u.Id == 0));
        
        var component = Ctx.RenderComponent<NavLayout>(parameters => parameters
            .AddCascadingValue<Register<IAsyncKeySubscriber>>(_ => true)
            .AddCascadingValue<Register<IAsyncPacketSubscriber>>(_ => true)
            .Add(p => p.UnregisterPacketSubscribers, _ => 0)
            .Add(p => p.UnregisterKeyPressSubscribers, _ => 0));

        await SetUser(null);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await component.Instance.OnUpdate(InteractionEvent.General));
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await component.Instance.GetFriendData());
        Assert.Throws<InvalidOperationException>(() => component.Instance.GetGroupData());
        Assert.Throws<InvalidOperationException>(() => component.Instance.GetDmData());
    }
}