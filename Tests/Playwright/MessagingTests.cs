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
        var user1Page = await SetupAndLoginAtFirstDm();
        var user2Context = await Browser!.NewContextAsync();
        var user2Page = await user2Context.NewPageAsync();
        await user2Page.GotoAsync(ServerAddress);
        await Login(user2Page, "testuser2", "testuser2@gmail.com");
        await OpenFirstDm(user2Page);
        
        const string message = "Hello, User1!";
        const string reply = "Hello, User2!";
        
        await user1Page
            .Locator("#message-input")
            .FillAsync(message);
        await Task.Delay(1000);
        await user1Page
            .Locator("#message-input")
            .PressAsync("Enter");
        await Task.Delay(500);
        
        var user1Message = await user1Page
            .Locator(".message-content")
            .Last
            .InnerTextAsync();
        Assert.Equal(message, user1Message, ignoreAllWhiteSpace: true);
        
        var user2Message = await user2Page
            .Locator(".message-content")
            .Last
            .InnerTextAsync();
        Assert.Equal(message, user2Message, ignoreAllWhiteSpace: true);
        
        await user2Page
            .Locator("#message-input")
            .FillAsync(reply);
        await Task.Delay(1000);
        await user2Page
            .Locator("#message-input")
            .PressAsync("Enter");
        await Task.Delay(500);
        
        user1Message = await user1Page
            .Locator(".message-content")
            .Last
            .InnerTextAsync();
        Assert.Equal(reply, user1Message, ignoreAllWhiteSpace: true);
        
        user2Message = await user2Page
            .Locator(".message-content")
            .Last
            .InnerTextAsync();
        Assert.Equal(reply, user2Message, ignoreAllWhiteSpace: true);
    }
}
