using System.Security.Claims;
using Bamboozlers.Classes.AppDbContext;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

public class MockAuthenticationProvider
{
    private readonly Mock<AuthenticationStateProvider> _mockAuthStateProvider;
    private User _user;
    
    public MockAuthenticationProvider(TestContextBase ctx, User user)
    {
        _user = user;
        _mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        
        _mockAuthStateProvider.Setup(x => x.GetAuthenticationStateAsync()).Returns(CreateAuthState());
        
        ctx.Services.AddSingleton(GetAuthStateProvider());
    }
    
    private async Task<AuthenticationState> CreateAuthState()
    {
        return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, _user.UserName!) }, "TestAuthType"))));
    }
    
    public void SetUser(User user)
    {
        _user = user;
    }
    
    public AuthenticationStateProvider GetAuthStateProvider()
    {
        return _mockAuthStateProvider.Object;
    }
}