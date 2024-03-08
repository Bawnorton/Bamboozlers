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
    private readonly User user;
    private readonly string fakePswd;
    
    public RegistrationTests(){
        userStoreMock = new Mock<IUserStore<User>>();
        userEmailStoreMock = userStoreMock.As<IUserEmailStore<User>>();
        userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
        redirectManagerMock = new Mock<IdentityRedirectManagerWrapper>(Mock.Of<IIdentityRedirectManager>());
        emailSenderMock = new Mock<IEmailSender<User>>();
        user = new User { Id = 1, UserName = "testUser", Email = "testUser@mail.it" };
        fakePswd = "TestPassword123!";
        
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
        
        Assert.Empty(page.FindAll("div.text-danger"));
        userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), fakePswd), Times.Once);
        emailSenderMock.Verify(x => 
            x.SendConfirmationLinkAsync(It.IsAny<User>(), user.Email!, It.IsAny<string>())
            , Times.Once);
        redirectManagerMock.Verify(r => r.RedirectTo(expectedUrl,expectedParameters),
            Times.Once,
            "Redirect was not called with the expected parameters.");
    }
    
    [Fact]
    public void TestInvalidUsername()
    {
        //empty username
        var page = Ctx.RenderComponent<Register>();
        page.Find("#email").Change(user.Email);
        page.Find("#password").Change(fakePswd);
        page.Find("#confirmPswd").Change(fakePswd);
        page.Find(".btn").Click();
        page.Find(".btn").Click();
        var expected = page.FindAll("div.text-danger");
        Assert.Single(expected);
        Assert.Equal("The Username field is required.", expected[0].InnerHtml);
        
        //invalid username
        page.Find("#username").Change("invalid__username!");
        page.Find(".btn").Click();
        expected = page.FindAll("div.text-danger");
        Assert.Single(expected);
        Assert.Equal("Username is invalid. It can only contain letters, numbers, and underscores. " +
                     "There can only be 1 underscore in a row.", expected[0].InnerHtml);
        
        //username already in use
        userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "DuplicateUserName", Description = "Username is already in use."
            }));
        page.Find("#username").Change(user.UserName);
        page.Find(".btn").Click();
        expected = page.FindAll("div.text-danger");
        Assert.Single(expected);
        Assert.Equal("Username is already in use.", expected[0].InnerHtml);
    }
    
    [Fact]
    public void TestInvalidEmail()
    {
        //empty email
        var page = Ctx.RenderComponent<Register>();
        page.Find("#username").Change(user.UserName);
        page.Find("#password").Change(fakePswd);
        page.Find("#confirmPswd").Change(fakePswd);
        page.Find(".btn").Click();
        var expected = page.FindAll("div.text-danger");
        Assert.Single(expected);
        Assert.Equal("The Email field is required.", expected[0].InnerHtml);
        
        //invalid email
        page.Find("#email").Change("invalidEmail");
        page.Find(".btn").Click();
        expected = page.FindAll("div.text-danger");
        Assert.Single(expected);
        Assert.Equal("Provided email address is invalid.", expected[0].InnerHtml);
        
        //email already in use
        userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "DuplicateEmail", Description = "Email is already in use."
            }));
        page.Find("#email").Change(user.Email);
        page.Find(".btn").Click();
        expected = page.FindAll("div.text-danger");
        Assert.Single(expected);
        Assert.Equal("Email is already in use.", expected[0].InnerHtml);
            
    }
    
    [Fact]
    public void TestInvalidPassword()
    {
        var invalidPswd = "invalidPswd";
        //empty password
        var page = Ctx.RenderComponent<Register>();
        page.Find("#email").Change(user.Email);
        page.Find("#username").Change(user.UserName);
        page.Find(".btn").Click();
        var expected = page.FindAll("div.text-danger");
        Assert.Single(expected);
        Assert.Equal("The Password field is required.", expected[0].InnerHtml);
        
        //passwords do not match
        page.Find("#password").Change(invalidPswd);
        page.Find(".btn").Click();
        expected = page.FindAll("div.text-danger");
        Assert.Single(expected);
        Assert.Equal("The password and confirmation password do not match.", expected[0].InnerHtml);
        
        
        //invalid password (no non-alphanumeric character)
        userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed( 
                new IdentityError
                {
                    Code = "PasswordRequiresNonAlphanumeric", 
                    Description = "Passwords must have at least one non-alphanumeric character."
                }));
        
        page.Find("#confirmPswd").Change(invalidPswd);
        page.Find(".btn").Click();
        expected = page.FindAll("div.text-danger");
        Assert.Single(expected);
        Assert.Equal("Passwords must have at least one non-alphanumeric character.", expected.FirstOrDefault()?.InnerHtml);
        
        
    }
}
