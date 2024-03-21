using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Events;
using Bamboozlers.Classes.Interop;
using Bamboozlers.Components.Chat;
using Microsoft.EntityFrameworkCore;
using Tests.Provider;

namespace Tests;

public class ChatTests : AuthenticatedBlazoriseTestBase
{
    public ChatTests()
    {
        _ = new MockEventServiceProvider(Ctx);
        _ = new MockJsRuntimeProvider(Ctx);
    }
    
    [Fact]
    public async void ComponentInitializesCorrectly()
    {
        await using var db = await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();
        var chat = db.Chats.Include(chat => chat.Messages).First();

        var component = Ctx.RenderComponent<CompChatView>(parameters => parameters
            .Add(p => p.Chat, chat));

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

    // TODO: Move over to ChatJsTests
    [Fact]
    public async Task SendMessageAddsMessageCorreclty()
    {
        var db = await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();
        var chat = db.Chats.Include(chat => chat.Messages).First();
        
        var component = Ctx.RenderComponent<CompChatView>(parameters => parameters
            .Add(p => p.Chat, chat));
        
        // Act
        var div = component.Find("#message-input");
        
        // I would simulate a keyboard input but apparently bunit doesn't support contenteditable elements, why? who knows, so instead we get this mess:
        Message? message = null;
        MessageEvents.MessageCreated.Register(created =>
        {
            message = created;
            message.ID = db.Messages.Count() + 1; 
            return Task.FromResult(message)!;
        });
        
        div.InnerHtml = "Test message";
        await SimulateInput("message-input", "Test message");
        
        // Calling StateHasChanged is supposed to re-render the component, but that also doesn't work for some reason
        component.Render();
        var messageContainer = component.Find("#message_" + message!.ID);
        var content = messageContainer.Children;
        var messageElement = content.FirstOrDefault(child => child.ClassList.Contains("message-content"))!.FirstChild;
        
        // Assert
        Assert.NotNull(messageElement);
        
        var expected = message.Content;
        var actual = messageElement.TextContent;
        
        // Assert
        Assert.Equal(expected, actual);
    }

    private static async Task SimulateInput(string id, string text)
    {
        var keyEvents = text.Select(KeyData.FromChar).ToList();
        var disallowed = await InputEvents.DisallowedInputs.Invoker().Invoke();
        var runningContent = "";
        foreach (var keyEvent in keyEvents)
        {
            var passed = !disallowed.Contains(keyEvent);
            await InputEvents.InputKeydown.Invoker().Invoke(id, keyEvent.key, keyEvent.code, keyEvent.ctrl,
                keyEvent.shift, keyEvent.alt, keyEvent.meta, runningContent, passed);
            if(passed) runningContent += keyEvent.key;
            await InputEvents.InputKeyup.Invoker().Invoke(id, keyEvent.key, keyEvent.code, keyEvent.ctrl,
                keyEvent.shift, keyEvent.alt, keyEvent.meta, runningContent, passed);
        }

        var enter = KeyData.Normal("Enter", "Enter");
        var enterDisallowed = disallowed.Contains(enter);
        await InputEvents.InputKeydown.Invoker().Invoke(id, enter.key, enter.code, enter.ctrl, enter.shift, enter.alt, enter.meta, runningContent, !enterDisallowed);
        if(!enterDisallowed) runningContent += "\n";
        await InputEvents.InputKeyup.Invoker().Invoke(id, enter.key, enter.code, enter.ctrl, enter.shift, enter.alt, enter.meta, runningContent, !enterDisallowed);
    }
    
    private static async Task ToKeyEvent(KeyData keyData)
    {
        await KeyboardEvents.Keydown.Invoker().Invoke(keyData.key, keyData.code, keyData.ctrl, keyData.shift, keyData.alt, keyData.meta);
        await KeyboardEvents.Keyup.Invoker().Invoke(keyData.key, keyData.code, keyData.ctrl, keyData.shift, keyData.alt, keyData.meta);
    }
}