using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Interop;
using Bamboozlers.Classes.Services;
using Bamboozlers.Classes.Services.Authentication;
using Blazorise.Modules;
using HttpContextMoq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
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
        MockAuthenticationProvider = new MockAuthenticationProvider(Ctx);
        MockJsRuntimeProvider = new MockJsRuntimeProvider(Ctx);
        MockWebSocketServiceProvider = new MockWebSocketServiceProvider(Ctx);
        MockUserManager = new MockUserManager(Ctx, MockDatabaseProvider);
        
        AuthService = new AuthService(MockAuthenticationProvider.GetAuthStateProvider(),MockDatabaseProvider.GetDbContextFactory());
        UserService = new UserService(AuthService, new ServiceProviderMock());
        
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