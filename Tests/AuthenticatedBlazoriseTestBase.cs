using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Networking;
using Bamboozlers.Classes.Services.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Tests.Provider;

namespace Tests;

public class AuthenticatedBlazoriseTestBase : BlazoriseTestBase
{
    protected MockDatabaseProvider MockDatabaseProvider;
    protected MockAuthenticationProvider MockAuthenticationProvider;
    protected MockJsRuntimeProvider MockJsRuntimeProvider;
    protected MockWebSocketServiceProvider MockWebSocketServiceProvider;
    protected MockUserManager MockUserManager;
    
    protected AuthService AuthService;
    protected UserService UserService;
    
    protected User? Self;

    protected AuthenticatedBlazoriseTestBase()
    {
        MockDatabaseProvider = new MockDatabaseProvider(Ctx);
        Self = MockDatabaseProvider.GetDbContextFactory().CreateDbContext().Users.First();
        MockAuthenticationProvider = new MockAuthenticationProvider(Ctx);
        MockJsRuntimeProvider = new MockJsRuntimeProvider(Ctx);
        MockWebSocketServiceProvider = new MockWebSocketServiceProvider(Ctx);
        MockUserManager = new MockUserManager(Ctx, MockDatabaseProvider);
        
        WebSocketHandler.Init();
        
        AuthService = new AuthService(MockAuthenticationProvider.GetAuthStateProvider(),MockDatabaseProvider.GetDbContextFactory());
        UserService = new UserService(AuthService, MockUserManager.GetUserManager());
        Ctx.Services.AddSingleton<IUserService>(UserService);
        Ctx.Services.AddSingleton<IAuthService>(AuthService);
    }

    protected async Task SetUser(User? user)
    {
        Self = user;
        await MockAuthenticationProvider.SetUser(user);
        UserService.Invalidate();
    }
}