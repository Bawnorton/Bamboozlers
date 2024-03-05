using Bamboozlers.Classes;
using Bamboozlers.Layout;
using Xunit.Abstractions;

namespace Tests;

public class NavLayoutTests : BlazoriseTestBase
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly MockDatabaseProvider _mockDatabaseProvider;
    private readonly MockAuthenticationProvider _mockAuthenticationProvider;
    
    public NavLayoutTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _mockDatabaseProvider = new MockDatabaseProvider(Ctx);
        _mockAuthenticationProvider = new MockAuthenticationProvider(Ctx);
        
        AuthHelper.Init(_mockAuthenticationProvider.GetAuthStateProvider(), _mockDatabaseProvider.GetDbContextFactory());
    }

    [Fact]
    public void TestInitialLoadingDisplay()
    {
        var component = Ctx.RenderComponent<NavLayout>();
        _testOutputHelper.WriteLine(component.Markup);
    }
}