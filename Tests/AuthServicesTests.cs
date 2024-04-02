using Bamboozlers.Classes.Services.UserServices;
using Bamboozlers.Classes.Utility.Observer;
using Tests.Provider;

namespace Tests;

public class AuthServicesTests : AuthenticatedBlazoriseTestBase
{
    public AuthServicesTests()
    {
        AuthService = new AuthService(MockAuthenticationProvider.GetAuthStateProvider(),MockDatabaseProvider.GetDbContextFactory());
        UserService = new UserService(AuthService, new MockServiceProviderWrapper(Ctx, MockUserManager).GetServiceProviderWrapper());
    }
    
    [Fact]
    public async Task AuthServicesTests_AuthService()
    {
        await SetUser((await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync())
            .Users.First(u => u.Id == 0));
        
        var claims = await AuthService.GetClaims();
        var identity = await AuthService.GetIdentity();

        Assert.NotNull(claims);
        Assert.NotNull(identity);

        var user = await AuthService.GetUser();
        Assert.NotNull(user);
        Assert.Equal(user, Self);
        Assert.True(await AuthService.IsAuthenticated());
        
        AuthService.Invalidate();
        Assert.False(AuthService.HasClaims());
        Assert.False(AuthService.HasIdentity());
        
        Assert.Equal(MockAuthenticationProvider.GetAuthStateProvider(), AuthService.AuthenticationStateProvider);
        Assert.Equal(MockDatabaseProvider.GetDbContextFactory(), AuthService.DbContextFactory);
    }

    [Fact]
    public async Task AuthServicesTests_UserService()
    {
        await SetUser((await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync())
            .Users.First(u => u.Id == 0));
        
        var dbContext = await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();

        var userData = await UserService.GetUserDataAsync();
        Assert.NotNull(userData);
        Assert.Equal(0, userData.Id);
        Assert.Equal("TestUser0",userData.UserName);
        Assert.Equal("test_user0@gmail.com",userData.Email);
        
        var user = dbContext.Users.First(u => u.Id == 1);
        await SetUser(user);
        
        userData = await UserService.GetUserDataAsync();
        Assert.NotNull(userData);
        Assert.Equal(1, userData.Id);
        Assert.Equal("TestUser1",userData.UserName);
        Assert.Equal("test_user1@gmail.com",userData.Email);
        
        var notified = false;
        var subscriber = new Mock<IUserSubscriber>();
        subscriber.Setup(x => x.OnUserUpdate()).Callback(() => notified = true);
        
        var result = UserService.AddSubscriber(subscriber.Object);
        Assert.True(result);
        Assert.Contains(subscriber.Object, UserService.Subscribers);
        
        result = UserService.AddSubscriber(subscriber.Object);
        Assert.False(result);
        
        user = dbContext.Users.First(u => u.Id == 2);
        await SetUser(user);
        await UserService.RebuildAndNotify();
        
        Assert.True(notified);
        
        userData = await UserService.GetUserDataAsync();
        Assert.NotNull(userData);
        Assert.Equal(2, userData.Id);
        Assert.Equal("TestUser2",userData.UserName);
        Assert.Equal("test_user2@gmail.com",userData.Email);

        result = ((IAsyncPublisher<IUserSubscriber>) UserService).RemoveSubscriber(subscriber.Object);
        Assert.True(result);
        Assert.DoesNotContain(subscriber.Object,UserService.Subscribers);
        
        subscriber = new Mock<IUserSubscriber>();
        result = ((IAsyncPublisher<IUserSubscriber>) UserService).RemoveSubscriber(subscriber.Object);
        Assert.False(result);
        
        await SetUser(null);
        var identityResult = await UserService.ChangePasswordAsync("", "");
        Assert.Equal("User not found.", identityResult.Errors.First().Description);
        
        identityResult = await UserService.DeleteAccountAsync("");
        Assert.Equal("User not found.", identityResult.Errors.First().Description);
        
        identityResult = await UserService.ChangeUsernameAsync("","");
        Assert.Equal("User not found.", identityResult.Errors.First().Description);
        
        identityResult = await UserService.UpdateUserAsync();
        Assert.Equal("User not found.", identityResult.Errors.First().Description);
    }
}