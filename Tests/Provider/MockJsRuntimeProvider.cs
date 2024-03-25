using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;

namespace Tests.Provider;

public class MockJsRuntimeProvider
{
    private readonly Mock<IJSRuntime> _mockJsRuntime;
    private readonly Mock<IJSObjectReference> _mockJsObjectReference;

    public MockJsRuntimeProvider(TestContext ctx)
    {
        _mockJsRuntime = new Mock<IJSRuntime>();
        _mockJsObjectReference = new Mock<IJSObjectReference>();

        _mockJsRuntime.Setup(x => x.InvokeAsync<IJSObjectReference>("import", It.IsAny<object[]?>()))
            .ReturnsAsync(_mockJsObjectReference.Object);

        _mockJsRuntime.Setup(x
            => x.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object[]>())
        ).Returns(new ValueTask<IJSVoidResult>());
        
        ctx.JSInterop.Mode = JSRuntimeMode.Strict;
        ctx.Services.AddSingleton(GetJsRuntime());
    }

    public IJSRuntime GetJsRuntime()
    {
        return _mockJsRuntime.Object;
    }
}