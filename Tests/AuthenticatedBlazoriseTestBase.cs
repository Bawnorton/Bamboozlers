using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Services;
using Tests.Provider;

namespace Tests;

public class AuthenticatedBlazoriseTestBase : BlazoriseTestBase
{
    protected readonly MockDatabaseProvider MockDatabaseProvider;
    protected readonly MockAuthenticationProvider MockAuthenticationProvider;
    protected readonly MockWebSocketServiceProvider MockWebSocketServiceProvider;
    protected readonly MockUserManager MockUserManager;
    protected readonly MockUserService MockUserService;

    protected readonly User Self;

    protected AuthenticatedBlazoriseTestBase()
    {
        MockDatabaseProvider = new MockDatabaseProvider(Ctx);
        Self = MockDatabaseProvider.GetDbContextFactory().CreateDbContext().Users.First();
        MockAuthenticationProvider = new MockAuthenticationProvider(Ctx, Self);
        MockWebSocketServiceProvider = new MockWebSocketServiceProvider(Ctx);
        MockUserManager = new MockUserManager(Ctx);
        MockUserService = new MockUserService(Ctx, MockAuthenticationProvider, MockDatabaseProvider, MockUserManager);
    }
}