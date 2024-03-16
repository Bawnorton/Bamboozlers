
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

public class MockServiceProviderWrapper
{
    private readonly Mock<ServiceProviderWrapper> _mockServices;
    private readonly Mock<IServiceScope> _mockServiceScope;
    private readonly MockUserManager _mockUserManager;
    
    public MockServiceProviderWrapper(TestContextBase ctx, MockUserManager mockUserManager)
    {
        _mockUserManager = mockUserManager;

        _mockServices = new Mock<ServiceProviderWrapper>(new Mock<IServiceProvider>().Object);
        _mockServiceScope = new Mock<IServiceScope>();
        ctx.Services.AddSingleton(_mockServices.Object);
        ctx.Services.AddSingleton(_mockServiceScope.Object);
        
        _mockServices.Setup(x => x.CreateScope()).Returns(_mockServiceScope.Object);
        _mockServices.Setup(x => x.GetService<UserManager<User>>()).Returns(_mockUserManager.GetUserManager());
    }

    public ServiceProviderWrapper GetServiceProviderWrapper()
    {
        return _mockServices.Object;
    }
}