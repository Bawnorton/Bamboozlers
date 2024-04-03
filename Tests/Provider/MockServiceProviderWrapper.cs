using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Provider;

public class MockServiceProviderWrapper
{
    private readonly Mock<ServiceProviderWrapper> _mockServices;

    public MockServiceProviderWrapper(TestContextBase ctx, MockUserManager mockUserManager)
    {
        _mockServices = new Mock<ServiceProviderWrapper>(new Mock<IServiceProvider>().Object);
        Mock<IServiceScope> mockServiceScope = new();
        ctx.Services.AddSingleton(_mockServices.Object);
        ctx.Services.AddSingleton(mockServiceScope.Object);

        var serviceProvider = ctx.Services.BuildServiceProvider();

        mockServiceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider);
        _mockServices.Setup(x => x.CreateScope()).Returns(mockServiceScope.Object);
        _mockServices.Setup(x => x.CreateAsyncScope()).Returns(new AsyncServiceScope(mockServiceScope.Object));
        _mockServices.Setup(x => x.GetService<UserManager<User>>()).Returns(mockUserManager.GetUserManager());
    }

    public ServiceProviderWrapper GetServiceProviderWrapper()
    {
        return _mockServices.Object;
    }
}