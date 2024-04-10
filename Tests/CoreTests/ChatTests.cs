using Bamboozlers.Components.Chat;
using Blazorise;
using Blazorise.Modules;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tests.Provider;

namespace Tests.CoreTests;

public class ChatTests : AuthenticatedBlazoriseTestBase
{
    public ChatTests()
    {
        Ctx.Services.AddSingleton(new Mock<IJSModalModule>().Object);
        Ctx.Services.AddBlazorise().Replace(ServiceDescriptor.Transient<IComponentActivator, ComponentActivator>());

        _ = new MockJsRuntimeProvider(Ctx);
    }

    [Fact]
    public async void ComponentInitializesCorrectly()
    {
        await SetUser(MockUserManager.CreateMockUser(0));
        await using var db = await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();
        var chat = db.Chats.Include(chat => chat.Messages).First();

        var component = Ctx.RenderComponent<CompChatView>(parameters => parameters
            .Add(p => p.AddPacketSubscriber, _ => true)
            .Add(p => p.AddKeySubscriber, _ => true));

        component.Instance.ChatID = chat.ID;
        
        // Assert
        Assert.NotNull(chat.Messages);

        var expectedCount = chat.Messages.Count;
        var actualCount = component.FindAll(".message-content").Count;

        // Assert
        Assert.Equal(expectedCount, actualCount);

        foreach (var chatMessage in chat.Messages)
        {
            var messageContainer = component.Find("#message_" + chatMessage.ID);
            var content = messageContainer.Children;
            var message = content.FirstOrDefault(child => child.ClassList.Contains("message-content"))!.FirstChild;

            // Assert
            Assert.NotNull(message);

            var expected = chatMessage.Content;
            var actual = message.TextContent;

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}