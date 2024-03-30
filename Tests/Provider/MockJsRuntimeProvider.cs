using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;

namespace Tests.Provider;

public class MockJsRuntimeProvider
{
    private readonly Mock<IJSRuntime> _mockJsRuntime;

    public MockJsRuntimeProvider(TestContext ctx)
    {
        _mockJsRuntime = new Mock<IJSRuntime>();
        Mock<IJSObjectReference> mockJsObjectReference = new();

        _mockJsRuntime.Setup(x => x.InvokeAsync<IJSObjectReference>("import", It.IsAny<object[]?>()))
            .ReturnsAsync(mockJsObjectReference.Object);

        _mockJsRuntime.Setup(x
            => x.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object[]>())
        ).Returns(new ValueTask<IJSVoidResult>());
        
        _mockJsRuntime.Setup(js => js.InvokeAsync<bool>("showConfirmDialog",It.IsAny<object[]>())
        ).Returns(ValueTask.FromResult(true));
        
        ctx.JSInterop.Mode = JSRuntimeMode.Strict;
        ctx.Services.AddSingleton(GetJsRuntime());
    }

    public IJSRuntime GetJsRuntime()
    {
        return _mockJsRuntime.Object;
    }

}