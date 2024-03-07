using Bamboozlers.Account;
using Bamboozlers.Account.Pages;
using Bamboozlers.Classes.AppDbContext;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

public class RegistrationTests: TestBase
{
    private readonly Mock<UserManager<User>> userManagerMock;
    private readonly Mock<IdentityRedirectManagerWrapper> redirectManagerMock;
    private readonly Mock<IEmailSender<User>> emailSenderMock;
    private readonly FakeNavigationManager navMan;
    private readonly Mock<IUserStore<User>> userStoreMock;
    private readonly Mock<IUserEmailStore<User>> userEmailStoreMock;
    
    public RegistrationTests(){
        userStoreMock = new Mock<IUserStore<User>>();
        userEmailStoreMock = userStoreMock.As<IUserEmailStore<User>>();
        userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
        redirectManagerMock = new Mock<IdentityRedirectManagerWrapper>(Mock.Of<IIdentityRedirectManager>());
        emailSenderMock = new Mock<IEmailSender<User>>();
        
        Ctx.Services.AddSingleton(userManagerMock.Object);
        Ctx.Services.AddSingleton(redirectManagerMock.Object);
        Ctx.Services.AddSingleton(userStoreMock.Object);
        Ctx.Services.AddSingleton(emailSenderMock.Object);
        Ctx.Services.AddSingleton<IUserStore<User>>(userEmailStoreMock.Object);
        navMan = Ctx.Services.GetRequiredService<FakeNavigationManager>();
    }

    [Fact]
    public void TestSuccessfulRegistration()
    {
        User user = new User { Id = 1, UserName = "testUser", Email = "testUser@mail.it" };
        string fakePswd = "TestPassword123!";
        string expectedUrl = "Account/RegisterConfirmation";
        var expectedParameters = new Dictionary<string, object?>
        {
            ["email"] = user.Email,
            ["returnUrl"] = "Account/Login"
        };
        
        userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), fakePswd))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(x => x.GetUserIdAsync(It.IsAny<User>()))
            .ReturnsAsync("1");
        userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("token");
        
        var page = Ctx.RenderComponent<Register>();
        var uri = navMan.GetUriWithQueryParameter("ReturnUrl", "Account/Login");
        navMan.NavigateTo(uri);
        
        page.Find("#email").Change(user.Email);
        page.Find("#username").Change(user.UserName);
        page.Find("#password").Change(fakePswd);
        page.Find("#confirmPswd").Change(fakePswd);
        page.Find(".btn").Click();
        
        userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), fakePswd), Times.Once);
        emailSenderMock.Verify(x => 
            x.SendConfirmationLinkAsync(It.IsAny<User>(), user.Email, It.IsAny<string>())
            , Times.Once);
        redirectManagerMock.Verify(r => r.RedirectTo(expectedUrl,expectedParameters),
            Times.Once,
            "Redirect was not called with the expected parameters.");
    }
}
