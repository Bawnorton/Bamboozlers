using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Events;
using Tests.Playwright.Infrastructure;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

namespace Tests.Playwright;

[Collection("Sequential")]
public class MessagingJsTests(CustomWebApplicationFactory fixture, ITestOutputHelper outputHelper) : PlaywrightTestBase(fixture, outputHelper)
{
    [Fact]
    public async Task TestMessageEcho()
    {
        var page = await SetupAndLoginAtFirstDm();

        await page
            .Locator("#message-input")
            .ClickAsync();
        await page
            .Locator("#message-input")
            .FillAsync("Hello, World!");
        await Task.Delay(1000);
        await page
            .Locator("#message-input")
            .PressAsync("Enter");
        await Task.Delay(1000);

        var color = await page
            .Locator(".message-content")
            .Last
            .EvaluateAsync("element => window.getComputedStyle(element).getPropertyValue('color')");

        Assert.Equal("rgb(222, 226, 230)", color.ToString()); // Color of the message is `rgb(160, 163, 167)` when sent and then updated to `rgb(222, 226, 230)` when recieved
    }

    [Fact]
    public async Task TestRealtimeMessaging()
    {
        var user1Page = await SetupAndLoginAtFirstDm(headless: false);
        
        await Task.Delay(1000);
        
        const string message = "Hello, User1!";
        const string reply = "Hello, User2!";
        
        var recievedMessage = false;
        var recievedReply = false;
        
        NetworkEvents.ReadDatabaseRequest.Register("messaging-test", (type) =>
        {
            Assert.Equal(type, DbEntry.ChatMessage);
            recievedMessage = true;
            return Task.CompletedTask;
        });
        
        await user1Page
            .Locator("#message-input")
            .FillAsync(message);
        await Task.Delay(1000);
        await user1Page
            .Locator("#message-input")
            .PressAsync("Enter");
        await Task.Delay(500);
        
        Assert.True(recievedMessage);

        await Task.Delay(500);
        
        var user2Context = await Browser!.NewContextAsync();
        var user2Page = await user2Context.NewPageAsync();
        await user2Page.GotoAsync(ServerAddress);
        await Login(user2Page, "testuser2", "testuser2@gmail.com");
        await OpenFirstDm(user2Page);
        
        NetworkEvents.ReadDatabaseRequest.Register("messaging-test", (type) =>
        {
            Assert.Equal(type, DbEntry.ChatMessage);
            recievedReply = true;
            return Task.CompletedTask;
        });
        
        await user2Page
            .Locator("#message-input")
            .FillAsync(reply);
        await Task.Delay(1000);
        await user2Page
            .Locator("#message-input")
            .PressAsync("Enter");
        await Task.Delay(1000);
        
        Assert.True(recievedReply);
    }
}
