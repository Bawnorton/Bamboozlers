using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Services.Authentication;
using Bamboozlers.Classes.Utility.Observer;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Tests;

public class AuthServicesTests : AuthenticatedBlazoriseTestBase
{
    public AuthServicesTests()
    {
        AuthService = new AuthService(MockAuthenticationProvider.GetAuthStateProvider(),MockDatabaseProvider.GetDbContextFactory());
        UserService = new UserService(AuthService, new MockUserManager(Ctx, MockDatabaseProvider).GetUserManager());
    }
    
    [Fact]
    public async Task AuthServicesTests_AuthService()
    {
        await SetUser((await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync())
            .Users.First(u => u.Id == 0));
        AuthService.Invalidate();
        
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
        UserService.Invalidate();
        
        var dbContext = await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();
        var claims = await AuthService.GetClaims();
        var user = await UserService.GetUserAsync(claims);
        Assert.NotNull(user);

        var userData = await UserService.GetUserDataAsync();
        Assert.NotNull(userData);
        Assert.Equal(0, userData.Id);
        Assert.Equal("TestUser0",userData.UserName);
        Assert.Equal("test_user0@gmail.com",userData.Email);
        
        user = dbContext.Users.First(u => u.Id == 1);
        await MockAuthenticationProvider.SetUser(user);
        UserService.Invalidate();
        
        userData = await UserService.GetUserDataAsync();
        Assert.NotNull(userData);
        Assert.Equal(1, userData.Id);
        Assert.Equal("TestUser1",userData.UserName);
        Assert.Equal("test_user1@gmail.com",userData.Email);
        
        var notified = false;
        var subscriber = new Mock<ISubscriber>();
        subscriber.Setup(x => x.OnUpdate()).Callback(() => notified = true);
        
        var result = UserService.AddSubscriber(subscriber.Object);
        Assert.True(result);
        Assert.Contains(subscriber.Object,UserService.Subscribers);
        
        result = UserService.AddSubscriber(subscriber.Object);
        Assert.False(result);
        
        user = dbContext.Users.First(u => u.Id == 2);
        await MockAuthenticationProvider.SetUser(user);
        await UserService.RebuildAndNotify();
        
        Assert.True(notified);
        
        userData = await UserService.GetUserDataAsync();
        Assert.NotNull(userData);
        Assert.Equal(2, userData.Id);
        Assert.Equal("TestUser2",userData.UserName);
        Assert.Equal("test_user2@gmail.com",userData.Email);

        result = UserService.RemoveSubscriber(subscriber.Object);
        Assert.True(result);
        Assert.DoesNotContain(subscriber.Object,UserService.Subscribers);
        
        subscriber = new Mock<ISubscriber>();
        result = UserService.RemoveSubscriber(subscriber.Object);
        Assert.False(result);
    }
}