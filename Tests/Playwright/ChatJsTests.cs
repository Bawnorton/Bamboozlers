using Microsoft.Playwright;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace Tests.Playwright;

[Parallelizable(ParallelScope.Self)]
public class ChatJsTests() : BlazorJsTestBase(headless: false)
{
    [Test]
    public async Task ChatJsTest()
    {
        await Task.Delay(10000);
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = " Direct Messages" }).ClickAsync();
    }
}