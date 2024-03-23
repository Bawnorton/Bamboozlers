using Bamboozlers.Classes.Services.UserService;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Provider;

public class MockUserService
{
    private readonly Mock<UserService> _mockUserService;
    
    public MockUserService(TestContextBase ctx, MockAuthenticationProvider mockAuthenticationProvider, MockDatabaseProvider mockDatabaseProvider, MockUserManager mockUserManager)
    {
        _mockUserService = new Mock<UserService>(
            mockAuthenticationProvider.GetAuthStateProvider(),
            mockDatabaseProvider.GetDbContextFactory(),
            mockUserManager.GetUserManager()
        );
        ctx.Services.AddSingleton<IUserService>(_mockUserService.Object);
    }

    public IUserService GetUserService()
    {
        return _mockUserService.Object;
    }
}