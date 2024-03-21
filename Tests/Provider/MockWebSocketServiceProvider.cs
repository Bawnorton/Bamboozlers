using Bamboozlers.Classes.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Provider;

public class MockWebSocketServiceProvider
{
    private readonly Mock<IWebSocketService> _mockWebSocketService;
    
    public MockWebSocketServiceProvider(TestContextBase ctx)
    {
        _mockWebSocketService = new Mock<IWebSocketService>();
        
        ctx.Services.AddSingleton(GetWebSocketService());
    }
    
    public IWebSocketService GetWebSocketService()
    {
        return _mockWebSocketService.Object;
    }
}