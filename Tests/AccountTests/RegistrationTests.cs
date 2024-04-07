using Bamboozlers.Account;
using Bamboozlers.Account.Pages;
using Bamboozlers.Classes.AppDbContext;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.AccountTests;

public class RegistrationTests : TestBase
{
    private readonly Mock<IEmailSender<User>> _emailSenderMock;
    private readonly string _fakePswd;
    private readonly FakeNavigationManager _navMan;
    private readonly Mock<IdentityRedirectManagerWrapper> _redirectManagerMock;
    private readonly User _user;
    private readonly Mock<UserManager<User>> _userManagerMock;

    public RegistrationTests()
    {
        var userStoreMock = new Mock<IUserStore<User>>();
        var userEmailStoreMock = userStoreMock.As<IUserEmailStore<User>>();
        _userManagerMock =
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        _redirectManagerMock = new Mock<IdentityRedirectManagerWrapper>(Mock.Of<IIdentityRedirectManager>());
        _emailSenderMock = new Mock<IEmailSender<User>>();
        _user = new User { Id = 1, UserName = "testUser", Email = "testUser@mail.it" };
        _fakePswd = "TestPassword123!";

        Ctx.Services.AddSingleton(_userManagerMock.Object);
        Ctx.Services.AddSingleton(_redirectManagerMock.Object);
        Ctx.Services.AddSingleton(userStoreMock.Object);
        Ctx.Services.AddSingleton(_emailSenderMock.Object);
        Ctx.Services.AddSingleton<IUserStore<User>>(userEmailStoreMock.Object);
        _navMan = Ctx.Services.GetRequiredService<FakeNavigationManager>();
    }

    // Helper method to perform common setup tasks
    private void SetupPageAndEnterValues(IRenderedComponent<Register> page, string username, string email,
        string password, string confirmPassword)
    {
        page.Find("#email").Change(email);
        page.Find("#username").Change(username);
        page.Find("#password").Change(password);
        page.Find("#confirmPswd").Change(confirmPassword);
        page.Find(".btn").Click();
    }

    private void SetupPageAndEnterUserValues(IRenderedComponent<Register> page)
    {
        SetupPageAndEnterValues(page, _user.UserName!, _user.Email!, _fakePswd, _fakePswd);
    }

    [Fact]
    public void TestSuccessfulRegistration()
    {
        var expectedUrl = "Account/RegisterConfirmation";
        var expectedParameters = new Dictionary<string, object?>
        {
            ["email"] = _user.Email,
            ["returnUrl"] = "Account/Login"
        };

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), _fakePswd))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.GetUserIdAsync(It.IsAny<User>()))
            .ReturnsAsync("1");
        _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("token");

        var page = Ctx.RenderComponent<Register>();
        var uri = _navMan.GetUriWithQueryParameter("ReturnUrl", "Account/Login");
        _navMan.NavigateTo(uri);

        SetupPageAndEnterUserValues(page);

        Assert.Empty(page.FindAll("div.text-danger"));
        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), _fakePswd), Times.Once);
        _emailSenderMock.Verify(x =>
                x.SendConfirmationLinkAsync(It.IsAny<User>(), _user.Email!, It.IsAny<string>())
            , Times.Once);
        _redirectManagerMock.Verify(r => r.RedirectTo(expectedUrl, expectedParameters),
            Times.Once,
            "Redirect was not called with the expected parameters.");
    }

    [Theory]
    [InlineData("", "The Username field is required.")]
    [InlineData("invalid__username!",
        "Username is invalid. It can only contain letters, numbers, and underscores. There can only be 1 underscore in a row.")]
    public void TestBasicInvalidUsername(string username, string errorMsg)
    {
        var page = Ctx.RenderComponent<Register>();
        SetupPageAndEnterValues(page, username, _user.Email!, _fakePswd, _fakePswd);
        var expected = page.FindAll("div.text-danger");
        Assert.Single(expected);
        Assert.Equal(errorMsg, expected[0].InnerHtml);
    }

    [Fact]
    public void TestUserNameInUse()
    {
        var page = Ctx.RenderComponent<Register>();
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
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
        SetupPageAndEnterValues(page, _user.UserName!, email, _fakePswd, _fakePswd);
        var expected = page.FindAll("div.text-danger");
        Assert.Single(expected);
        Assert.Equal(errorMsg, expected[0].InnerHtml);
    }

    [Fact]
    public void TestEmailInUse()
    {
        var page = Ctx.RenderComponent<Register>();
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
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
        SetupPageAndEnterValues(page, _user.UserName!, _user.Email!, password, confirmPassword);

        var expected = page.FindAll("div.text-danger");
        Assert.Single(expected);
        Assert.Equal(expectedErrorMessage, expected[0].InnerHtml);
    }

    [Fact]
    public void TestComplexPasswordValidation()
    {
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordRequiresNonAlphanumeric",
                Description = "Passwords must have at least one non-alphanumeric character."
            }));

        var page = Ctx.RenderComponent<Register>();
        SetupPageAndEnterUserValues(page);
        var expected = page.FindAll("div.text-danger");
        Assert.Single(expected);
        Assert.Equal("Passwords must have at least one non-alphanumeric character.",
            expected.FirstOrDefault()?.InnerHtml);
    }
}