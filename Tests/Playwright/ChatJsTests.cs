using Tests.Helpers;
using Tests.Playwright.Infrastructure;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

namespace Tests.Playwright;

[Collection("Sequential")]
public class ChatJsTests(CustomWebApplicationFactory fixture, ITestOutputHelper outputHelper) : PlaywrightTestBase(fixture, outputHelper) 
{
    [Fact]
    public async Task SendMessageContent()
    {
        var page = await SetupAndLoginAtFirstDm();

        var message = TextHelper.RandomText();
        
        await page
            .Locator("#message-input")
            .ClickAsync();
        await page
            .Locator("#message-input")
            .FillAsync(message);
        await Task.Delay(1000);
        await page
            .Locator("#message-input")
            .PressAsync("Enter");
        await Task.Delay(1000);

        var content = await page
            .Locator(".message-content")
            .Last
            .InnerTextAsync();
        
        Assert.Equal(message, content, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task ShiftEnterDoesNotSendMessage()
    {
        var page = await SetupAndLoginAtFirstDm();
        
        await Task.Delay(1000);
        var beforeMessageCount = await page
            .Locator(".message-content")
            .CountAsync();

        await page
            .Locator("#message-input")
            .ClickAsync();
        await page
            .Locator("#message-input")
            .FillAsync("Hello");
        await Task.Delay(1000);
        await page.Keyboard.DownAsync("Shift");
        await page.Keyboard.PressAsync("Enter");
        await page.Keyboard.UpAsync("Shift");
        await page.Keyboard.TypeAsync("World");

        var afterMessageCount = await page
            .Locator(".message-content")
            .CountAsync();

        Assert.Equal(beforeMessageCount, afterMessageCount);

        var content = await page
            .Locator("#message-input")
            .InputValueAsync();

        Assert.Equal("Hello\nWorld", content, ignoreAllWhiteSpace: true);
    }
}