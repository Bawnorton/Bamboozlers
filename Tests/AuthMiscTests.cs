using Bamboozlers.Account;
using Bamboozlers.Account.Pages;
using Bamboozlers.Classes.AppDbContext;
using Bunit.TestDoubles;
using HttpContextMoq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

public class AuthMiscTests : TestBase
{
    private readonly Mock<IEmailSender<User>> _emailSenderMock;
    private readonly HttpContextMock _httpContextMock;
    private readonly FakeNavigationManager _navMan;
    private readonly Mock<UserManager<User>> _userManagerMock;

    public AuthMiscTests()
    {
        // Setup code that is shared across tests
        _userManagerMock =
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
        _emailSenderMock = new Mock<IEmailSender<User>>();
        var redirectManagerMock = new Mock<IdentityRedirectManagerWrapper>(Mock.Of<IIdentityRedirectManager>());
        var signInManagerMock = new Mock<SignInManager<User>>(_userManagerMock.Object, Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<User>>(), null, null, null, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        _httpContextMock = new HttpContextMock();

        Ctx.Services.AddSingleton(_userManagerMock.Object);
        Ctx.Services.AddSingleton(redirectManagerMock.Object);
        Ctx.Services.AddSingleton(signInManagerMock.Object);
        Ctx.Services.AddSingleton(_emailSenderMock.Object);
        _navMan = Ctx.Services.GetRequiredService<FakeNavigationManager>();
    }

    [Fact]
    public void TestConfirmEmail()
    {
        // Pre-navigate to simulate query parameters in the URL.
        var uri = _navMan.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            { "Code", "someCode" },
            { "UserId", "1" }
        });
        _navMan.NavigateTo(uri);

        // Mock user manager responses
        _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(new User());
        _userManagerMock.Setup(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Now render the component after setting up the navigation context
        var page = Ctx.RenderComponent<ConfirmEmail>(
            p => p.AddCascadingValue<HttpContext>(_httpContextMock));

        // Asserts
        Assert.Contains("Thank you for confirming your email. ", page.Markup);
        Assert.Contains("click here to login.", page.Markup);
        Assert.Contains("Account/Login", page.Markup);
    }

    [Fact]
    public void TestConfirmEmailChange()
    {
        // Pre-navigate to simulate query parameters in the URL.
        var uri = _navMan.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            { "Code", "someCode" },
            { "UserId", "1" },
            { "Email", "test@email.it" }
        });
        _navMan.NavigateTo(uri);

        // Mock user manager responses
        _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(new User());
        _userManagerMock.Setup(x => x.ChangeEmailAsync(
            It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

        // Now render the component after setting up the navigation context
        var page = Ctx.RenderComponent<ConfirmEmailChange>(
            p => p.AddCascadingValue<HttpContext>(_httpContextMock));

        // Asserts
        Assert.Contains("Thank you for confirming your email change.", page.Markup);
    }

    [Fact]
    public void TestResendEmailConfirm()
    {
        var user = new User();
        const string testEmail = "test@example.com";

        _userManagerMock.Setup(x => x.FindByEmailAsync(testEmail)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetUserIdAsync(user)).ReturnsAsync("1");
        _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user)).ReturnsAsync("SomeToken");

        var page = Ctx.RenderComponent<ResendEmailConfirmation>(parameters =>
            parameters.AddCascadingValue(_httpContextMock));
        page.Find("#email").Change(testEmail);
        page.Find(".btn").Click();
        // Assert
        _emailSenderMock.Verify(x => x.SendConfirmationLinkAsync(user, testEmail, It.IsAny<string>()), Times.Once());
        _userManagerMock.VerifyAll();
        Assert.Contains("Verification email sent. Please check your email.", page.Markup);
    }

    [Fact]
    public void TestForgotPassword()
    {
        var user = new User();
        const string testEmail = "test@example.com";

        _userManagerMock.Setup(x => x.FindByEmailAsync(testEmail)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.IsEmailConfirmedAsync(user)).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("SomeToken");

        var page = Ctx.RenderComponent<ForgotPassword>(parameters =>
            parameters.AddCascadingValue(_httpContextMock));
        page.Find("#email").Change(testEmail);
        page.Find(".btn").Click();
        // Assert
        _emailSenderMock.Verify(x => x.SendPasswordResetLinkAsync(user, testEmail, It.IsAny<string>()), Times.Once());
        _userManagerMock.VerifyAll();
        Assert.Contains(
            "If an account with that email exists, you will receive an email with instructions on how to reset your password.",
            page.Markup);
    }

    [Fact]
    public void TestResetPasswordSuccess()
    {
        var uri = _navMan.GetUriWithQueryParameter("Code", "someCode");
        _navMan.NavigateTo(uri);

        _userManagerMock.Setup(x => x.FindByEmailAsync("user@example.com")).ReturnsAsync(new User());
        _userManagerMock.Setup(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), "NewPassword123!"))
            .ReturnsAsync(IdentityResult.Success);

        var component = Ctx.RenderComponent<ResetPassword>(parameters =>
            parameters.AddCascadingValue(_httpContextMock));

        // Simulate user input
        component.Find("#email").Change("user@example.com");
        component.Find("#password").Change("NewPassword123!");
        component.Find("#confirmpswd").Change("NewPassword123!");

        // Act
        component.Find(".btn").Click();

        // Assert
        Assert.Contains("Your password has been reset.", component.Markup);
    }
}