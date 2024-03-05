using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

public class MockAuthenticationProvider
{
    private readonly Mock<AuthenticationStateProvider> _mockAuthStateProvider;
    
    public MockAuthenticationProvider(TestContextBase ctx, string userName)
    {
        _mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        
        var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, userName) }, "TestAuthType"))));
        _mockAuthStateProvider.Setup(x => x.GetAuthenticationStateAsync()).Returns(authState);
        
        ctx.Services.AddSingleton(GetAuthStateProvider());
    }

    public AuthenticationStateProvider GetAuthStateProvider()
    {
        return _mockAuthStateProvider.Object;
    }
}