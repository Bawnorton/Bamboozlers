using Bamboozlers.Account;
using Bamboozlers.Account.Pages;
using Bamboozlers.Classes.AppDbContext;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

public class RegistrationTests: TestBase
{
    private readonly Mock<UserManager<User>> userManagerMock;
    private readonly Mock<IdentityRedirectManagerWrapper> redirectManagerMock;
    private readonly Mock<IEmailSender<User>> emailSenderMock;
    private readonly FakeNavigationManager navMan;
    private readonly User user;
    private readonly string fakePswd;
    
    public RegistrationTests(){
        var userStoreMock = new Mock<IUserStore<User>>();
        var userEmailStoreMock = userStoreMock.As<IUserEmailStore<User>>();
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
    
    // Helper method to perform common setup tasks
    private void SetupPageAndEnterValues(IRenderedComponent<Register> page, string username, string email, string password, string confirmPassword)
    {
        page.Find("#email").Change(email);
        page.Find("#username").Change(username);
        page.Find("#password").Change(password);
        page.Find("#confirmPswd").Change(confirmPassword);
        page.Find(".btn").Click();
    }

    private void SetupPageAndEnterUserValues(IRenderedComponent<Register> page)
    {
        SetupPageAndEnterValues(page, user.UserName!, user.Email!, fakePswd, fakePswd);
    }

    [Fact]
    public void TestSuccessfulRegistration()
    {
        var expectedUrl = "Account/RegisterConfirmation";
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
        
        SetupPageAndEnterUserValues(page);
        
        Assert.Empty(page.FindAll("div.text-danger"));
        userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), fakePswd), Times.Once);
        emailSenderMock.Verify(x => 
            x.SendConfirmationLinkAsync(It.IsAny<User>(), user.Email!, It.IsAny<string>())
            , Times.Once);
        redirectManagerMock.Verify(r => r.RedirectTo(expectedUrl,expectedParameters),
            Times.Once,
            "Redirect was not called with the expected parameters.");
    }
    
    [Theory]
    [InlineData("", "The Username field is required.")]
    [InlineData("invalid__username!", "Username is invalid. It can only contain letters, numbers, and underscores. There can only be 1 underscore in a row.")]
    public void TestBasicInvalidUsername(string username, string errorMsg)
    {
        var page = Ctx.RenderComponent<Register>();
        SetupPageAndEnterValues(page, username, user.Email!, fakePswd, fakePswd);;
        var expected = page.FindAll("div.text-danger");
        Assert.Single(expected);
        Assert.Equal(errorMsg, expected[0].InnerHtml);
    }

    [Fact]
    public void TestUserNameInUse()
    {
        var page = Ctx.RenderComponent<Register>();
        userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "DuplicateUserName", Description = "Username is already in use."
            }));
        
        SetupPageAndEnterUserValues(page);
        
        var expected = page.FindAll("div.text-danger");
        Assert.Single(expected);
        Assert.Equal("Username is already in use.", expected[0].InnerHtml);
    }
    
    [Theory]
    [InlineData("", "The Email field is required.")]
    [InlineData("invalidEmail", "Provided email address is invalid.")]
    public void TestInvalidEmail(string email, string errorMsg)
    {
        var page = Ctx.RenderComponent<Register>();
        SetupPageAndEnterValues(page, user.UserName!, email, fakePswd, fakePswd);
        var expected = page.FindAll("div.text-danger");
        Assert.Single(expected);
        Assert.Equal(errorMsg, expected[0].InnerHtml);
    }

    [Fact]
    public void TestEmailInUse()
    {
        var page = Ctx.RenderComponent<Register>();
        userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "DuplicateEmail", Description = "Email is already in use."
            }));
        SetupPageAndEnterUserValues(page);
        var expected = page.FindAll("div.text-danger");
        Assert.Single(expected);
        Assert.Equal("Email is already in use.", expected[0].InnerHtml);
    }

    [Theory]
    [InlineData("", "", "The Password field is required.")]
    [InlineData("invalidPswd", "", "The password and confirmation password do not match.")]
    public void TestBasicPasswordValidation(string password, string confirmPassword, string expectedErrorMessage)
    {
        var page = Ctx.RenderComponent<Register>();
        SetupPageAndEnterValues(page, user.UserName!, user.Email!, password, confirmPassword);
    
        var expected = page.FindAll("div.text-danger");
        Assert.Single(expected);
        Assert.Equal(expectedErrorMessage, expected[0].InnerHtml);
    }
    
    [Fact]
    public void TestComplexPasswordValidation()
    {
        userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordRequiresNonAlphanumeric",
                Description = "Passwords must have at least one non-alphanumeric character."
            }));
    
        var page = Ctx.RenderComponent<Register>();
        SetupPageAndEnterUserValues(page);
        var expected = page.FindAll("div.text-danger");
        Assert.Single(expected);
        Assert.Equal("Passwords must have at least one non-alphanumeric character.", expected.FirstOrDefault()?.InnerHtml);
    }
}
