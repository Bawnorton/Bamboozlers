using System.Text;
using Microsoft.Playwright;
using Tests.Playwright.Infrastructure;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

namespace Tests.Playwright;

public class ChatJsTests : PlaywrightTestBase
{
    private ITestOutputHelper ITestOutputHelper { get; }
    
    public ChatJsTests(CustomWebApplicationFactory fixture, ITestOutputHelper testOutputHelper) : base(fixture)
    {
        ITestOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public async Task SendMessageContent()
    {
        using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = "../../../../auth.json"
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(ServerAddress);
        
        var random = new Random();
        
        await OpenFirstDm(page);

        var message = RandomText(random);
        while (message.StartsWith('\n')) message = message[1..];
        while (message.EndsWith('\n')) message = message[..^1];
        
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
        using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = "../../../../auth.json"
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(ServerAddress);

        await OpenFirstDm(page);

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
            .InnerTextAsync();
        
        Assert.Equal("Hello\nWorld", content, ignoreAllWhiteSpace: true);
    }
    
    private static async Task OpenFirstDm(IPage page)
    {
        await page
            .GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = " Direct Messages" })
            .ClickAsync();
        await page
            .Locator("#dms_dropdown")
            .Locator(".b-bar-item")
            .First
            .ClickAsync();
    }

    private static string RandomText(Random random)
    {
        var text = new StringBuilder();
        for (var i = 0; i < 500; i++)
        {
            text.Append((char) random.Next(32, 127));
            if (random.Next(0, 100) < 3)
            {
                text.Append('\n');
            }
        }

        return text.ToString();
    }
}