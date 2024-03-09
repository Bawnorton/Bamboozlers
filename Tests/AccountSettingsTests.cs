using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Services;

namespace Tests;

public class AccountSettingsTests : BlazoriseTestBase
{
    private readonly MockDatabaseProvider _mockDatabaseProvider;
    private readonly MockAuthenticationProvider _mockAuthenticationProvider;
    private readonly MockUserManager _mockUserManager;

    private readonly User _self;
    
    public AccountSettingsTests()
    {
        _mockDatabaseProvider = new MockDatabaseProvider(Ctx);
        _self = _mockDatabaseProvider.GetDbContextFactory().CreateDbContext().Users.First();
        _mockAuthenticationProvider = new MockAuthenticationProvider(Ctx, _self.UserName!);
        _mockUserManager = new MockUserManager(Ctx);

        AuthHelper.Init(_mockAuthenticationProvider.GetAuthStateProvider(), _mockDatabaseProvider.GetDbContextFactory());
    }
}