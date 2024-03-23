using NUnit.Framework;
using Tests.Playwright.Infrastructure;

namespace Tests.Playwright;

[Parallelizable(ParallelScope.Self)]
public class PlaywrightTestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly string ServerAddress;
    
    protected PlaywrightTestBase(CustomWebApplicationFactory fixture)
    {
        ServerAddress = fixture.ServerAddress;
    }
}