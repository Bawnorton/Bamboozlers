using Bamboozlers.Classes.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Provider;

public class MockEventServiceProvider
{
    private readonly Mock<IEventService> _mockEventService = new();

    public MockEventServiceProvider(TestContext ctx)
    {
        ctx.Services.AddSingleton(GetEventService());
    }
    
    public IEventService GetEventService()
    {
        return _mockEventService.Object;
    }
}