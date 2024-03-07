using Bamboozlers.Account;
using Bamboozlers.Classes.AppDbContext;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

public class RegistrationTests: TestBase
{
    private readonly Mock<SignInManager<User>> signInManagerMock;
    private readonly Mock<UserManager<User>> userManagerMock;
    private readonly Mock<IdentityRedirectManagerWrapper> redirectManagerMock;
    private readonly FakeNavigationManager navMan;
    private readonly Mock<IUserStore<User>> userStoreMock;
    
    public RegistrationTests(){
        userManagerMock = new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
        signInManagerMock = new Mock<SignInManager<User>>(userManagerMock.Object, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<User>>(), null, null, null, null);
        redirectManagerMock = new Mock<IdentityRedirectManagerWrapper>(Mock.Of<IIdentityRedirectManager>());
        userStoreMock = new Mock<IUserStore<User>>();
        
        Ctx.Services.AddSingleton(userManagerMock.Object);
        Ctx.Services.AddSingleton(signInManagerMock.Object);
        Ctx.Services.AddSingleton(redirectManagerMock.Object);
        Ctx.Services.AddSingleton(userStoreMock.Object);
        navMan = Ctx.Services.GetRequiredService<FakeNavigationManager>();
    }

    [Fact]
    public void TestSuccessfulRegistration()
    {
    }
}