using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Networking;
using Bamboozlers.Classes.Services;
using Tests.Provider;

namespace Tests;

public class AuthenticatedBlazoriseTestBase : BlazoriseTestBase
{
    protected readonly MockDatabaseProvider MockDatabaseProvider;
    protected readonly MockAuthenticationProvider MockAuthenticationProvider;

    protected readonly User Self;

    protected AuthenticatedBlazoriseTestBase()
    {
        MockDatabaseProvider = new MockDatabaseProvider(Ctx);
        Self = MockDatabaseProvider.GetDbContextFactory().CreateDbContext().Users.First();
        MockAuthenticationProvider = new MockAuthenticationProvider(Ctx, Self);
        
        AuthHelper.Init(MockAuthenticationProvider.GetAuthStateProvider(), MockDatabaseProvider.GetDbContextFactory());
        WebSocketHandler.Init(AuthHelper.GetSelf().Id);
    }
}