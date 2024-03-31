using Bamboozlers.Classes.AppDbContext;
using Blazorise;
using Blazorise.Modules;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tests.Provider;
using Xunit.Abstractions;

namespace Tests;

using Bamboozlers.Components.Chat;

public class GroupChatTests : AuthenticatedBlazoriseTestBase
{
    private ITestOutputHelper output;
    public GroupChatTests(ITestOutputHelper outputHelper)
    {
        output = outputHelper;
        //MockDatabaseProvider.SetupMockDbContext();
        Ctx.Services.AddSingleton(new Mock<IJSModalModule>().Object);
        Ctx.Services.AddBlazorise().Replace(ServiceDescriptor.Transient<IComponentActivator, ComponentActivator>());
        
        _ = new MockJsRuntimeProvider(Ctx);
    }
    
    [Fact]
    public async void GroupChatTests_CompAddMember()
    {
        var user0 = MockUserManager.CreateMockUser(0);
        var user1 = MockUserManager.CreateMockUser(1);
        var user2 = MockUserManager.CreateMockUser(0);
        
        var groupChat = new GroupChat
        {
            Owner = user0!,
            OwnerID = user0!.Id,
            Users = [
                user0,
                user2
            ],
            Moderators = [
                user1
            ]
        };
        var fakeDbContext = MockDatabaseProvider.GetDbContextFactory().CreateDbContext();
        await SetUser(fakeDbContext.Users.First());
        
        var component = Ctx.RenderComponent<CompAddMember>(
            parameters => parameters.Add(p => p.ChatID, groupChat.ID)
        );

        output.WriteLine(component.Markup);
        // Check observed Chats for component
        Assert.Equal(groupChat.ID, component.Instance.WatchedIDs[0]);
        
        // Assert: Check that the proper users are being displayed (friends)
    }

    [Fact]
    public async void GroupChatTests_CompChatSettings()
    {
        
    }

    [Fact]
    public async void GroupChatTests_CompChatView()
    {
        
    }
}