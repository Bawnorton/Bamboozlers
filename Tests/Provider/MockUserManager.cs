using Bamboozlers.Classes.AppDbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

public class MockUserManager
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    
    public MockUserManager(TestContextBase ctx)
    {
        _mockUserManager = new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
        ctx.Services.AddSingleton(_mockUserManager.Object);
    }

    public UserManager<User> GetUserManager()
    {
        return _mockUserManager.Object;
    }
}