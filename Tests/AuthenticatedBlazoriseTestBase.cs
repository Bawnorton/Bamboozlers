using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Services;
using Bamboozlers.Classes.Services.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Tests.Provider;

namespace Tests;

public class AuthenticatedBlazoriseTestBase : BlazoriseTestBase
{
    protected MockDatabaseProvider MockDatabaseProvider;
    protected MockAuthenticationProvider MockAuthenticationProvider;
    protected MockJsRuntimeProvider MockJsRuntimeProvider;
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
        MockUserManager = new MockUserManager(Ctx, MockDatabaseProvider);
        
        AuthService = new AuthService(MockAuthenticationProvider.GetAuthStateProvider(),MockDatabaseProvider.GetDbContextFactory());
        UserService = new UserService(AuthService, MockUserManager.GetUserManager());
        Ctx.Services.AddSingleton<IUserService>(UserService);
        Ctx.Services.AddSingleton<IAuthService>(AuthService);
        Ctx.Services.AddScoped<IKeyPressService, KeyPressService>();
    }

    protected async Task SetUser(User? user)
    {
        Self = user;
        await MockAuthenticationProvider.SetUser(user);
        UserService.Invalidate();
    }
}