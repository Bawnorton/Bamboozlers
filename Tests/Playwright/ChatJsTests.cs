using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using Tests.Playwright.Infrastructure;

namespace Tests.Playwright;

[Parallelizable(ParallelScope.Self)]
public class ChatJsTests(CustomWebApplicationFactory fixture) : PageTest, IClassFixture<CustomWebApplicationFactory>
{
    private readonly string _serverAddress = fixture.ServerAddress;

    [Fact]
    public async Task ChatJsTest()
    {
        using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });
        var page = await browser.NewPageAsync();
        
        await page.GotoAsync(_serverAddress);
        await Task.Delay(10000000);
    }
}