using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Services;
using Bamboozlers.Classes.Services.UserServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Tests.Provider;

namespace Tests;

public class AuthenticatedBlazoriseTestBase : BlazoriseTestBase
{
    protected MockDatabaseProvider MockDatabaseProvider;
    protected MockAuthenticationProvider MockAuthenticationProvider;
    protected MockUserManager MockUserManager;
    
    protected AuthService AuthService;
    protected UserInteractionService UserInteractionService;
    protected UserGroupService UserGroupService;
    protected UserService UserService;

    protected User? Self;

    protected AuthenticatedBlazoriseTestBase()
    {
        MockDatabaseProvider = new MockDatabaseProvider(Ctx);
        Self = MockDatabaseProvider.GetDbContextFactory().CreateDbContext().Users.First();
        MockAuthenticationProvider = new MockAuthenticationProvider(Ctx);
        _ = new MockJsRuntimeProvider(Ctx);
        MockUserManager = new MockUserManager(Ctx, MockDatabaseProvider);
        var mockHttpContextAccessor = new Mock<HttpContextAccessor>();

        AuthService = new AuthService(MockAuthenticationProvider.GetAuthStateProvider(), 
            mockHttpContextAccessor.Object,
            MockDatabaseProvider.GetDbContextFactory()
        );
        UserInteractionService = new UserInteractionService(AuthService, MockDatabaseProvider.GetDbContextFactory());
        UserGroupService = new UserGroupService(AuthService, 
            UserInteractionService,
            MockDatabaseProvider.GetDbContextFactory()
        );
        UserService = new UserService(AuthService,
            new MockServiceProviderWrapper(Ctx, MockUserManager).GetServiceProviderWrapper(), 
            MockDatabaseProvider.GetDbContextFactory(),
            UserGroupService
        );

        Ctx.Services.AddSingleton<IUserService>(UserService);
        Ctx.Services.AddSingleton<IAuthService>(AuthService);
        Ctx.Services.AddScoped<IKeyPressService, KeyPressService>();
        Ctx.Services.AddSingleton<IUserInteractionService>(UserInteractionService);
        Ctx.Services.AddSingleton<IUserGroupService>(UserGroupService);
    }

    protected async Task SetUser(User? user)
    {
        Self = user;
        await MockAuthenticationProvider.SetUser(user);
        UserService.Invalidate();
    }
}