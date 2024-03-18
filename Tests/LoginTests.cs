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


public class LoginTests : TestBase
{
    private readonly Mock<SignInManager<User>> signInManagerMock;
    private readonly Mock<UserManager<User>> userManagerMock;
    private readonly Mock<IdentityRedirectManagerWrapper> redirectManagerMock;
    private readonly FakeNavigationManager navMan;
    private readonly User user;
    private readonly User unconfirmedUser;
    private readonly HttpContextMock httpContextMock;
    private string fakePswd = "TestPassword123!";

    public LoginTests()
    {
        // Setup code that is shared across tests
        userManagerMock = new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
        signInManagerMock = new Mock<SignInManager<User>>(userManagerMock.Object, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<User>>(), null, null, null, null);
        redirectManagerMock = new Mock<IdentityRedirectManagerWrapper>(Mock.Of<IIdentityRedirectManager>());
        httpContextMock = new HttpContextMock();
        
        Ctx.Services.AddSingleton(userManagerMock.Object);
        Ctx.Services.AddSingleton(signInManagerMock.Object);
        Ctx.Services.AddSingleton(redirectManagerMock.Object);
        navMan = Ctx.Services.GetRequiredService<FakeNavigationManager>();

        user = new User { UserName = "testUser", Email = "testUser@example.com" };
        unconfirmedUser = new User { UserName = "unconfirmedUser", Email = "unconfirmed@example.com" };
        
        // Mock setups that are common across multiple tests
        userManagerMock.Setup(x => x.IsEmailConfirmedAsync(user)).ReturnsAsync(true);
        userManagerMock.Setup(x => x.FindByNameAsync(user.UserName)).ReturnsAsync(user);
        userManagerMock.Setup(x => x.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        signInManagerMock.Setup(x => x.PasswordSignInAsync(
            user.UserName, fakePswd, It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Success);
        signInManagerMock.Setup(x => x.PasswordSignInAsync(
                user.UserName, It.Is<string>(password => password != fakePswd), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Failed);
        
        
        userManagerMock.Setup(x => x.IsEmailConfirmedAsync(unconfirmedUser)).ReturnsAsync(false);
        userManagerMock.Setup(x => x.FindByNameAsync(unconfirmedUser.UserName)).ReturnsAsync(unconfirmedUser);
        userManagerMock.Setup(x => x.FindByEmailAsync(unconfirmedUser.Email)).ReturnsAsync(unconfirmedUser);
        signInManagerMock.Setup(x => x.PasswordSignInAsync(
            unconfirmedUser.UserName, fakePswd, It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Failed);
    }

    [Fact]
    public void TestAuthenticationSuccessWUsername()
    {
        var page = Ctx.RenderComponent<Login>(
            p => p.AddCascadingValue<HttpContext>(httpContextMock));
        var uri = navMan.GetUriWithQueryParameter("ReturnUrl", "/");
        navMan.NavigateTo(uri);
        
        page.Find("#email").Change(user.UserName);
        page.Find("#password").Change(fakePswd);
        page.Find(".btn").Click();
        
        // Assert
        signInManagerMock.Verify(
            x => x.PasswordSignInAsync(
                It.Is<string>(username => username == user.UserName), 
                It.Is<string>(password => password == fakePswd), 
                It.IsAny<bool>(), It.IsAny<bool>()), 
            Times.Once, 
            "Expected PasswordSignInAsync to be called once with correct credentials."
        );

    }
    
    [Fact]
    public void TestAuthenticationSuccessWEmail()
    {
        var page = Ctx.RenderComponent<Login>(
            p => p.AddCascadingValue<HttpContext>(httpContextMock));
        var uri = navMan.GetUriWithQueryParameter("ReturnUrl", "/");
        navMan.NavigateTo(uri);
        
        page.Find("#email").Change(user.Email);
        page.Find("#password").Change(fakePswd);
        page.Find(".btn").Click();
        
        // Assert
        signInManagerMock.Verify(
            x => x.PasswordSignInAsync(
                It.Is<string>(username => username == user.UserName), 
                It.Is<string>(password => password == fakePswd), 
                It.IsAny<bool>(), It.IsAny<bool>()), 
            Times.Once, 
            "Expected PasswordSignInAsync to be called once with correct credentials."
        );

    }

    [Fact]
    public void TestAuthenticationFailWUsername()
    {
        var page = Ctx.RenderComponent<Login>(
            p => p.AddCascadingValue<HttpContext>(httpContextMock));
        var uri = navMan.GetUriWithQueryParameter("ReturnUrl", "/");
        navMan.NavigateTo(uri);
        
        string badUsr = "badUser";
        page.Find("#email").Change(badUsr);
        page.Find("#password").Change(fakePswd);
        page.Find(".btn").Click();
        
        // Assert
        userManagerMock.Verify(
            x => x.FindByNameAsync(
                It.Is<string>(username => username == badUsr)), 
            Times.Once
        );

        string badEmail = "badUser@mail.com";
        page.Find("#email").Change(badEmail);
        page.Find("#password").Change(fakePswd);
        page.Find(".btn").Click();
        
        // Assert
        userManagerMock.Verify(
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
            p => p.AddCascadingValue<HttpContext>(httpContextMock));
        var uri = navMan.GetUriWithQueryParameter("ReturnUrl", "/");
        navMan.NavigateTo(uri);
        
        string badPswd = "badPswd";
        page.Find("#email").Change(user.Email);
        page.Find("#password").Change(badPswd);
        page.Find(".btn").Click();
        
        // Assert
        signInManagerMock.Verify(
            x => x.PasswordSignInAsync(
                It.Is<string>(username => username == user.UserName), 
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
            p => p.AddCascadingValue<HttpContext>(httpContextMock));
        var uri = navMan.GetUriWithQueryParameter("ReturnUrl", "/");
        navMan.NavigateTo(uri);
        
        page.Find("#email").Change(unconfirmedUser.Email);
        page.Find("#password").Change(fakePswd);
        page.Find(".btn").Click();
        
        // Assert
        signInManagerMock.Verify(
            x => x.PasswordSignInAsync(
                It.Is<string>(username => username == unconfirmedUser.UserName), 
                It.Is<string>(password => password == fakePswd), 
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
            p => p.AddCascadingValue<HttpContext>(httpContextMock));
        var uri = navMan.GetUriWithQueryParameter("ReturnUrl", "/");
        navMan.NavigateTo(uri);
        
        page.Find("#email").Change(user.UserName);
        page.Find("#password").Change(fakePswd);
        page.Find(".btn").Click();
        
        redirectManagerMock.Verify(r => r.RedirectTo("/"), Times.Once);
    }
}