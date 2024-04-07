using Bamboozlers.Account;
using Bamboozlers.Account.Pages;
using Bamboozlers.Classes.AppDbContext;
using Bunit.TestDoubles;
using HttpContextMoq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.AccountTests;

public class LoginTests : TestBase
{
    private readonly HttpContextMock _httpContextMock;
    private readonly FakeNavigationManager _navMan;
    private readonly Mock<IdentityRedirectManagerWrapper> _redirectManagerMock;
    private readonly Mock<SignInManager<User>> _signInManagerMock;
    private readonly User _unconfirmedUser;
    private readonly User _user;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private const string FakePswd = "TestPassword123!";

    public LoginTests()
    {
        // Setup code that is shared across tests
        _userManagerMock =
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
        _signInManagerMock = new Mock<SignInManager<User>>(_userManagerMock.Object, Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<User>>(), null, null, null, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        _redirectManagerMock = new Mock<IdentityRedirectManagerWrapper>(Mock.Of<IIdentityRedirectManager>());
        _httpContextMock = new HttpContextMock();

        Ctx.Services.AddSingleton(_userManagerMock.Object);
        Ctx.Services.AddSingleton(_signInManagerMock.Object);
        Ctx.Services.AddSingleton(_redirectManagerMock.Object);
        _navMan = Ctx.Services.GetRequiredService<FakeNavigationManager>();

        _user = new User { UserName = "testUser", Email = "testUser@example.com" };
        _unconfirmedUser = new User { UserName = "unconfirmedUser", Email = "unconfirmed@example.com" };

        // Mock setups that are common across multiple tests
        _userManagerMock.Setup(x => x.IsEmailConfirmedAsync(_user)).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.FindByNameAsync(_user.UserName)).ReturnsAsync(_user);
        _userManagerMock.Setup(x => x.FindByEmailAsync(_user.Email)).ReturnsAsync(_user);
        _signInManagerMock.Setup(x => x.PasswordSignInAsync(
                _user.UserName, FakePswd, It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Success);
        _signInManagerMock.Setup(x => x.PasswordSignInAsync(
                _user.UserName, It.Is<string>(password => password != FakePswd), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Failed);


        _userManagerMock.Setup(x => x.IsEmailConfirmedAsync(_unconfirmedUser)).ReturnsAsync(false);
        _userManagerMock.Setup(x => x.FindByNameAsync(_unconfirmedUser.UserName)).ReturnsAsync(_unconfirmedUser);
        _userManagerMock.Setup(x => x.FindByEmailAsync(_unconfirmedUser.Email)).ReturnsAsync(_unconfirmedUser);
        _signInManagerMock.Setup(x => x.PasswordSignInAsync(
                _unconfirmedUser.UserName, FakePswd, It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Failed);
    }

    [Fact]
    public void TestAuthenticationSuccessWUsername()
    {
        var page = Ctx.RenderComponent<Login>(
            p => p.AddCascadingValue<HttpContext>(_httpContextMock));
        var uri = _navMan.GetUriWithQueryParameter("ReturnUrl", "/");
        _navMan.NavigateTo(uri);

        page.Find("#email").Change(_user.UserName);
        page.Find("#password").Change(FakePswd);
        page.Find(".btn").Click();

        // Assert
        _signInManagerMock.Verify(
            x => x.PasswordSignInAsync(
                It.Is<string>(username => username == _user.UserName),
                It.Is<string>(password => password == FakePswd),
                It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Once,
            "Expected PasswordSignInAsync to be called once with correct credentials."
        );
    }

    [Fact]
    public void TestAuthenticationSuccessWEmail()
    {
        var page = Ctx.RenderComponent<Login>(
            p => p.AddCascadingValue<HttpContext>(_httpContextMock));
        var uri = _navMan.GetUriWithQueryParameter("ReturnUrl", "/");
        _navMan.NavigateTo(uri);

        page.Find("#email").Change(_user.Email);
        page.Find("#password").Change(FakePswd);
        page.Find(".btn").Click();

        // Assert
        _signInManagerMock.Verify(
            x => x.PasswordSignInAsync(
                It.Is<string>(username => username == _user.UserName),
                It.Is<string>(password => password == FakePswd),
                It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Once,
            "Expected PasswordSignInAsync to be called once with correct credentials."
        );
    }

    [Fact]
    public void TestAuthenticationFailWUsername()
    {
        var page = Ctx.RenderComponent<Login>(
            p => p.AddCascadingValue<HttpContext>(_httpContextMock));
        var uri = _navMan.GetUriWithQueryParameter("ReturnUrl", "/");
        _navMan.NavigateTo(uri);

        const string badUsr = "badUser";
        page.Find("#email").Change(badUsr);
        page.Find("#password").Change(FakePswd);
        page.Find(".btn").Click();

        // Assert
        _userManagerMock.Verify(
            x => x.FindByNameAsync(
                It.Is<string>(username => username == badUsr)),
            Times.Once
        );

        var badEmail = "badUser@mail.com";
        page.Find("#email").Change(badEmail);
        page.Find("#password").Change(FakePswd);
        page.Find(".btn").Click();

        // Assert
        _userManagerMock.Verify(
            x => x.FindByEmailAsync(
                It.Is<string>(email => email == badEmail)),
            Times.Once
        );

        var errorMsg = page.Find(".alert").TextContent;
        Assert.Equal("Error: Invalid login attempt.", errorMsg);
    }

    [Fact]
    public void TestAuthenticationFailWPswd()
    {
        var page = Ctx.RenderComponent<Login>(
            p => p.AddCascadingValue<HttpContext>(_httpContextMock));
        var uri = _navMan.GetUriWithQueryParameter("ReturnUrl", "/");
        _navMan.NavigateTo(uri);

        const string badPswd = "badPswd";
        page.Find("#email").Change(_user.Email);
        page.Find("#password").Change(badPswd);
        page.Find(".btn").Click();

        // Assert
        _signInManagerMock.Verify(
            x => x.PasswordSignInAsync(
                It.Is<string>(username => username == _user.UserName),
                It.Is<string>(password => password == badPswd),
                It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Once
        );

        var errorMsg = page.Find(".alert").TextContent;
        Assert.Equal("Error: Invalid login attempt.", errorMsg);
    }

    [Fact]
    public void TestUnConfirmedEmail()
    {
        var page = Ctx.RenderComponent<Login>(
            p => p.AddCascadingValue<HttpContext>(_httpContextMock));
        var uri = _navMan.GetUriWithQueryParameter("ReturnUrl", "/");
        _navMan.NavigateTo(uri);

        page.Find("#email").Change(_unconfirmedUser.Email);
        page.Find("#password").Change(FakePswd);
        page.Find(".btn").Click();

        // Assert
        _signInManagerMock.Verify(
            x => x.PasswordSignInAsync(
                It.Is<string>(username => username == _unconfirmedUser.UserName),
                It.Is<string>(password => password == FakePswd),
                It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Once
        );

        var errorMsg = page.Find(".alert").TextContent;
        Assert.Contains("Please confirm your email before logging in.", errorMsg);
    }

    [Fact]
    public void TestRedirectionAfterLogin()
    {
        var page = Ctx.RenderComponent<Login>(
            p => p.AddCascadingValue<HttpContext>(_httpContextMock));
        var uri = _navMan.GetUriWithQueryParameter("ReturnUrl", "/");
        _navMan.NavigateTo(uri);

        page.Find("#email").Change(_user.UserName);
        page.Find("#password").Change(FakePswd);
        page.Find(".btn").Click();

        _redirectManagerMock.Verify(r => r.RedirectTo("/"), Times.Once);
    }
}