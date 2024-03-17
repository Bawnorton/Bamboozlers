using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace Tests.Playwright;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ChatJsTests : BlazorJsTestBase
{
    [Test]
    public void ChatJsTest()
    {
        Assert.Pass();
    }
}