using Bamboozlers.Classes.AppDbContext;
<<<<<<< HEAD
using Bamboozlers.Classes.Networking;
=======
using Bamboozlers.Classes.Interop;
>>>>>>> main
using Bamboozlers.Classes.Services;
using Bamboozlers.Classes.Services.Authentication;
using Blazorise.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Tests.Provider;

namespace Tests;

public class AuthenticatedBlazoriseTestBase : BlazoriseTestBase
{
<<<<<<< HEAD
    protected readonly MockDatabaseProvider MockDatabaseProvider;
    protected readonly MockAuthenticationProvider MockAuthenticationProvider;

    protected readonly User Self;
=======
    protected MockDatabaseProvider MockDatabaseProvider;
    protected MockAuthenticationProvider MockAuthenticationProvider;
    protected MockJsRuntimeProvider MockJsRuntimeProvider;
    protected MockWebSocketServiceProvider MockWebSocketServiceProvider;
    protected MockUserManager MockUserManager;
    
    protected AuthService AuthService;
    protected UserService UserService;
    
    protected User? Self;
>>>>>>> main

    protected AuthenticatedBlazoriseTestBase()
    {
        MockDatabaseProvider = new MockDatabaseProvider(Ctx);
<<<<<<< HEAD
        Self = MockDatabaseProvider.GetDbContextFactory().CreateDbContext().Users.First();
        MockAuthenticationProvider = new MockAuthenticationProvider(Ctx, Self);
        
        AuthHelper.Init(MockAuthenticationProvider.GetAuthStateProvider(), MockDatabaseProvider.GetDbContextFactory());
        WebSocketHandler.Init();
=======
        MockAuthenticationProvider = new MockAuthenticationProvider(Ctx);
        MockJsRuntimeProvider = new MockJsRuntimeProvider(Ctx);
        MockWebSocketServiceProvider = new MockWebSocketServiceProvider(Ctx);
        MockUserManager = new MockUserManager(Ctx, MockDatabaseProvider);
        
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
>>>>>>> main
    }
}