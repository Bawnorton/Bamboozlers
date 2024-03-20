using Bamboozlers.Classes;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Services;
using Tests.Provider;

namespace Tests;

public class AuthenticatedBlazoriseTestBase : BlazoriseTestBase
{
    protected readonly MockDatabaseProvider MockDatabaseProvider;

    protected readonly User Self;

    protected AuthenticatedBlazoriseTestBase()
    {
        MockDatabaseProvider = new MockDatabaseProvider(Ctx);
        Self = MockDatabaseProvider.GetDbContextFactory().CreateDbContext().Users.First();
        var mockAuthenticationProvider = new MockAuthenticationProvider(Ctx, Self);

        AuthHelper.Init(mockAuthenticationProvider.GetAuthStateProvider(), MockDatabaseProvider.GetDbContextFactory());
    }
}