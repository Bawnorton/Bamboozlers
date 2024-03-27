using Bamboozlers.Components.Chat;
using Blazorise;
using Blazorise.Modules;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tests.Provider;

namespace Tests;

public class ChatTests : AuthenticatedBlazoriseTestBase
{
    public ChatTests()
    {
        MockDatabaseProvider.SetupMockDbContext();
        Ctx.Services.AddSingleton(new Mock<IJSModalModule>().Object);
        Ctx.Services.AddBlazorise().Replace(ServiceDescriptor.Transient<IComponentActivator, ComponentActivator>());
        
        _ = new MockJsRuntimeProvider(Ctx);
    }
    
    [Fact]
    public async void ComponentInitializesCorrectly()
    {
        await SetUser(MockDatabaseProvider.GetMockUser(0));
        await using var db = await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();
        var chat = db.Chats.Include(chat => chat.Messages).First();

        var component = Ctx.RenderComponent<CompChatView>(parameters => parameters
            .Add(p => p.ChatID, chat.ID));

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

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async void TestAddMembers(int userId)
    {
        await SetUser(MockDatabaseProvider.GetMockUser(userId));
        await using var db = await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();
        var chat = db.Chats.Include(chat => chat.Messages).Last();
        
        var component = Ctx.RenderComponent<CompChatView>(parameters => parameters
            .Add(p => p.ChatID, chat.ID));
        
        // Act
        var addMembersButton = component.FindAll("#addMembers");
        if (userId == 2)
        {
            Assert.Empty(addMembersButton);
            return;
        }
        addMembersButton.Single().Click();
        
        // Assert
        var checkboxes = component.FindAll("input[type='checkbox']");
        if (userId == 0)
        {
            Assert.Empty(checkboxes);
        }
        else
        {
             Assert.Single(checkboxes);
        }
       
        
        Assert.Equal(3, db.Chats.Include(chat => chat.Users).Last().Users.Count);

        // Act
        if(userId == 1)
        {
            checkboxes.Single().Change(true);
        }
        component.Find("#saveChanges").Click();
        if(userId == 1)
        {        
            Assert.Contains(db.Users.Last().UserName, component.Markup);
        }
        else
        {
            Assert.Contains("No member(s) added!", component.Markup);
        }

    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async void TestRemoveMembers(int userId)
    {
        await SetUser(MockDatabaseProvider.GetMockUser(userId));
        await using var db = await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();
        var chat = db.Chats.Include(chat => chat.Messages).Last();
        
        var component = Ctx.RenderComponent<CompChatView>(parameters => parameters
            .Add(p => p.ChatID, chat.ID));
        
        var removeMembersButton = component.FindAll("#removeMember");
        if (userId == 2)
        {
            Assert.Empty(removeMembersButton);
            return;
        }
        Assert.Equal(2, removeMembersButton.Count);
        removeMembersButton.First().Click();
        
        
        Assert.Equal(2, db.Chats.Include(chat => chat.Users).Last().Users.Count);
        
    }
    
    [Fact]
    public async void TestChatSettingsPic()
    {
        
        await SetUser(MockDatabaseProvider.GetMockUser(0));
        await using var db = await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();
        var chat = db.Chats.Include(chat => chat.Messages).Last();
        
        var component = Ctx.RenderComponent<CompChatSettings>(parameters => parameters
            .Add(p => p.Chat, chat));
        
        // Arrange: No file passed
        var spoofArgs = new InputFileChangeEventArgs(new List<IBrowserFile>());
        // Act
        await component.Instance.OnFileUpload(spoofArgs);
        // Assert
        Assert.Equal("Unable to change avatar. No file was uploaded.", component.Instance.AlertMessage);
        
        // Arrange: Invalid file passed (not an image)
        var fakeFile = new MockBrowserFile { ContentType = "file/csv" };
        spoofArgs = new InputFileChangeEventArgs(new List<IBrowserFile> { fakeFile });
        // Act
        await component.Instance.OnFileUpload(spoofArgs);
        // Assert
        Assert.Equal("Unable to change avatar. Uploaded file was not an image.", component.Instance.AlertMessage);
        
        // Arrange: Invalid file passed (image, but not png)
        fakeFile = new MockBrowserFile { ContentType = "image/gif" };
        spoofArgs = new InputFileChangeEventArgs(new List<IBrowserFile> { fakeFile });
        // Act
        await component.Instance.OnFileUpload(spoofArgs);
        // Assert
        Assert.Equal("Unable to change avatar. Avatar must be a PNG, or JPG file.", component.Instance.AlertMessage);
        
        // Arrange: Valid file passed, but image was empty
        fakeFile = new MockBrowserFile { ContentType = "image/png", Bytes = Array.Empty<byte>()};
        spoofArgs = new InputFileChangeEventArgs(new List<IBrowserFile> { fakeFile });
        // Act
        await component.Instance.OnFileUpload(spoofArgs);
        // Assert
        Assert.Equal("Unable to change avatar. An error occurred while processing uploaded avatar.", component.Instance.AlertMessage);
        
        // Arrange: Valid file passed
        fakeFile = new MockBrowserFile { ContentType = "image/png"};
        spoofArgs = new InputFileChangeEventArgs(new List<IBrowserFile> { fakeFile });
        // Act
        await component.Instance.OnFileUpload(spoofArgs);
        // Assert
        Assert.False(component.Instance.AlertVisible);
    }

    
}