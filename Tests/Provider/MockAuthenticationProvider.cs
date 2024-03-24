using System.Security.Claims;
using Bamboozlers.Classes.AppDbContext;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Provider;

public class MockAuthenticationProvider
{
    private readonly Mock<AuthenticationStateProvider> _mockAuthStateProvider;
    private AuthenticationState _authenticationState;
    private User _user;
    
    public MockAuthenticationProvider(TestContextBase ctx)
    {
        _mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        _mockAuthStateProvider.Setup(x => x.GetAuthenticationStateAsync()).ReturnsAsync(() => _authenticationState!);
        
        ctx.Services.AddSingleton(GetAuthStateProvider());
    }
    
    private async Task CreateAuthState()
    {
        _authenticationState = await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, _user.UserName!) }, "TestAuthType"))));
    }
    
    public async Task SetUser(User user)
    {
        _user = user;
        await CreateAuthState();
    }
    
    public AuthenticationStateProvider GetAuthStateProvider()
    {
        return _mockAuthStateProvider.Object;
    }
}