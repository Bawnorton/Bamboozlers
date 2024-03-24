using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Func;
using Bamboozlers.Classes.Services.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Provider;

public class MockAuthService
{
    private readonly Mock<AuthService> _mockAuthService;
    private readonly MockAuthenticationProvider _mockAuthenticationProvider;
    private readonly MockDatabaseProvider _mockDatabaseProvider;
    
    public MockAuthService(TestContextBase ctx,
        AuthenticatedBlazoriseTestBase testBase,
        MockAuthenticationProvider mockAuthenticationProvider,
        MockDatabaseProvider mockDatabaseProvider)
    {
        _mockAuthenticationProvider = mockAuthenticationProvider;
        _mockDatabaseProvider = mockDatabaseProvider;
        
        _mockAuthService = new Mock<AuthService>(
            _mockAuthenticationProvider.GetAuthStateProvider(),
            _mockDatabaseProvider.GetDbContextFactory()
        );
        
        _mockAuthService.Setup(x => x.GetClaims())
            .ReturnsAsync(
                _mockAuthenticationProvider.GetAuthStateProvider().GetAuthenticationStateAsync().Result.User
            );
        
        ctx.Services.AddSingleton<IAuthService>(_mockAuthService.Object);
    }

    public void SetUser(User user)
    {
        GetAuthService().Invalidate();
    }

    public AuthService GetAuthService()
    {
        return _mockAuthService.Object;
    }
}