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
    private readonly Mock<UserManager<User>> userManagerMock;
    private readonly Mock<IdentityRedirectManagerWrapper> redirectManagerMock;
    private readonly HttpContextMock httpContextMock;
    private readonly FakeNavigationManager navMan;

    public AuthMiscTests()
    {
        // Setup code that is shared across tests
        userManagerMock = new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
        redirectManagerMock = new Mock<IdentityRedirectManagerWrapper>(Mock.Of<IIdentityRedirectManager>());
        var signInManagerMock = new Mock<SignInManager<User>>(userManagerMock.Object, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<User>>(), null, null, null, null);

        httpContextMock = new HttpContextMock();
        
        Ctx.Services.AddSingleton(userManagerMock.Object);
        Ctx.Services.AddSingleton(redirectManagerMock.Object);
        Ctx.Services.AddSingleton(signInManagerMock.Object);
        navMan = Ctx.Services.GetRequiredService<FakeNavigationManager>();
    }
    
    [Fact]
    public void TestConfirmEmail()
    {
        // Pre-navigate to simulate query parameters in the URL.
        var uri = navMan.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            { "Code", "someCode" },
            { "UserId", "1" }
        });
        navMan.NavigateTo(uri);

        // Mock user manager responses
        userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(new User());
        userManagerMock.Setup(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

        // Now render the component after setting up the navigation context
        var page = Ctx.RenderComponent<ConfirmEmail>(
            p => p.AddCascadingValue<HttpContext>(httpContextMock));

        // Asserts
        Assert.Contains("Thank you for confirming your email. ", page.Markup);
        Assert.Contains("click here to login.", page.Markup);
        Assert.Contains("Account/Login", page.Markup);
    }
    
    [Fact]
    public void TestConfirmEmailChange()
    {
        // Pre-navigate to simulate query parameters in the URL.
        var uri = navMan.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            { "Code", "someCode" },
            { "UserId", "1" },
            { "Email", "test@email.it"}
        });
        navMan.NavigateTo(uri);

        // Mock user manager responses
        userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(new User());
        userManagerMock.Setup(x => x.ChangeEmailAsync(
            It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

        // Now render the component after setting up the navigation context
        var page = Ctx.RenderComponent<ConfirmEmailChange>(
            p => p.AddCascadingValue<HttpContext>(httpContextMock));

        // Asserts
        Assert.Contains("Thank you for confirming your email change.", page.Markup);
    }

}